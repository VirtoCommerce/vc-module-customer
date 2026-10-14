import { SecurityClient, SettingClient, useApiClient } from "@vc-shell/framework";
import { CustomerModuleClient, OrganizationMembershipClient, type Organization } from "../api_client/virtocommerce.customer";

export const customerApi = () => useApiClient(CustomerModuleClient).getApiClient();
export const membershipApi = () => useApiClient(OrganizationMembershipClient).getApiClient();
export const securityApi = () => useApiClient(SecurityClient).getApiClient();

export type MemberType = "Organization" | "Contact" | "Employee" | "Vendor";

// Same registry as the AngularJS memberTypesResolverService: icon, details blade, status setting, allowed children
export const MEMBER_TYPES: Record<
  MemberType,
  { icon: string; blade: string; statusSetting: string; children?: MemberType[] }
> = {
  Organization: {
    icon: "lucide-building-2",
    blade: "CustomerOrganizationDetails",
    statusSetting: "Customer.OrganizationStatuses",
    children: ["Organization", "Employee", "Contact"],
  },
  Contact: { icon: "lucide-user", blade: "CustomerContactDetails", statusSetting: "Customer.ContactStatuses" },
  Employee: { icon: "lucide-id-card", blade: "CustomerEmployeeDetails", statusSetting: "Customer.EmployeeStatuses" },
  Vendor: { icon: "lucide-briefcase", blade: "CustomerVendorDetails", statusSetting: "Customer.VendorStatuses" },
};

export const fullTypeName = (type: string) => `VirtoCommerce.CustomerModule.Core.Model.${type}`;

// Platform endpoints the generated clients don't cover (same-origin cookie auth)
export async function getJson<T>(url: string): Promise<T> {
  const res = await fetch(url, { headers: { Accept: "application/json" } });
  if (!res.ok) throw new Error(`${res.status} ${url}`);
  return res.json();
}

export interface Country {
  id: string;
  name: string;
}

let countries: Promise<Country[]> | undefined;
export const getCountries = () => (countries ??= getJson<Country[]>("/api/platform/common/countries"));
export const getRegions = (countryId: string) =>
  getJson<Country[]>(`/api/platform/common/countries/${encodeURIComponent(countryId)}/regions`).then((x) => x ?? []);

// no platform endpoint: the AngularJS admin listed moment-timezone names client-side
export const timeZones = (Intl as unknown as { supportedValuesOf(key: string): string[] }).supportedValuesOf("timeZone");

// a setting's values through settings/values/{name}, which needs only a signed-in user (legacy settings.getValues)
export const getSettingValues = (name: string) =>
  getJson<unknown[] | null>(`/api/platform/settings/values/${encodeURIComponent(name)}`)
    .then((x) => (x ?? []).map(String))
    .catch(() => [] as string[]);

// a setting's default needs platform:setting:read, as legacy settings.get did; without it there is none
export const getSettingDefault = (name: string) =>
  useApiClient(SettingClient)
    .getApiClient()
    .then((client) => client.getGlobalSetting(name))
    .then((setting) => setting.defaultValue ?? undefined)
    .catch(() => undefined);

// Date-only fields: the picker works in local days, the API stores UTC midnight (as the AngularJS datepicker did)
export const utcDayToLocal = (value?: Date | string | null) => {
  if (!value) return null;
  const d = new Date(value);
  return new Date(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate());
};
export const localDayToUtc = (value?: Date | string | null) => {
  if (!value) return undefined;
  const d = new Date(value);
  return new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
};

export const toOptions = (values: (string | undefined | null)[]) =>
  [...new Set(values.filter(Boolean) as string[])].map((x) => ({ id: x, title: x }));

/**
 * VcSelect options function over organizations (deep search); `exclude` names organizations to leave out of the
 * search results. Picked ids always resolve.
 */
export const organizationOptions =
  (exclude?: () => (string | undefined)[] | Promise<(string | undefined)[]>) =>
  async (keyword?: string, skip?: number, ids?: string[]): Promise<{ results: Organization[]; totalCount: number }> => {
    const client = await customerApi();
    if (ids?.length) {
      const results = await client.getOrganizationsByIds(ids);
      return { results, totalCount: results.length };
    }
    const [excluded, result] = await Promise.all([
      exclude?.() ?? [],
      client.searchOrganizations({ keyword, deepSearch: true, skip: skip ?? 0, take: 20 }),
    ]);
    return { results: (result.results ?? []).filter((o) => !excluded.includes(o.id)), totalCount: result.totalCount ?? 0 };
  };

/**
 * VcSelect options function for security roles limited by a whitelist setting (role names or ids;
 * empty = all roles), with the store's own value taking precedence (the AngularJS rolesPickerService).
 * `selected` resolves labels of the roles already picked; `toRoles` maps picked ids back to role objects.
 */
export function rolesOptions(
  whitelistSetting: string,
  selected: () => { roleId?: string; roleName?: string }[],
  storeId?: string,
) {
  let whitelist: Promise<string[]> | undefined;
  const getWhitelist = () =>
    (whitelist ??= (async () => {
      if (storeId) {
        const values = await getJson<Record<string, unknown>>(
          `/api/platform/settings/v2/tenant/Store/${encodeURIComponent(storeId)}/values`,
        ).catch(() => ({}) as Record<string, unknown>);
        const storeList = values[whitelistSetting];
        if (Array.isArray(storeList) && storeList.length) return storeList.map(String);
      }
      return getSettingValues(whitelistSetting);
    })());

  // names of the roles the select has listed, for roles picked from it
  const names = new Map<string, string>();

  async function search(keyword?: string, skip?: number, ids?: string[]) {
    if (ids?.length) {
      const picked = ids.map((id) => ({ id, name: selected().find((r) => r.roleId === id)?.roleName ?? id }));
      return { results: picked, totalCount: picked.length };
    }
    const [list, roles] = await Promise.all([
      getWhitelist(),
      (await securityApi()).searchRoles({ keyword, skip: skip ?? 0, take: 50 }),
    ]);
    const allowed = list.map((x) => x.toLowerCase());
    const results = (roles.results ?? [])
      .filter((r) => !allowed.length || allowed.includes(r.name!.toLowerCase()) || allowed.includes(r.id!.toLowerCase()))
      .map((r) => ({ id: r.id!, name: r.name! }));
    results.forEach((r) => names.set(r.id, r.name));
    // ponytail: whitelisted lists stop at the first 50 matches; page through filtered results if whitelists grow
    return { results, totalCount: allowed.length ? results.length : (roles.totalCount ?? 0) };
  }

  /** Maps picked role ids back to role objects, keeping the existing ones (and their ids). */
  const toRoles = <T extends { roleId?: string; roleName?: string }>(ids: string[], current: T[] = []) =>
    ids.map((id) => current.find((r) => r.roleId === id) ?? ({ roleId: id, roleName: names.get(id) ?? id } as T));

  return { search, toRoles };
}

/**
 * Uploads a member icon to the member-icons asset folder (the AngularJS member-icon blade).
 * Two alternating names per member keep the saved icon intact until the member itself is saved,
 * so the name is picked from the saved icon url, never from an upload not saved yet.
 */
export async function uploadMemberIcon(file: File, memberId: string | undefined, savedUrl?: string) {
  const ext = file.name.split(".").pop();
  const tail = savedUrl?.includes("-1.") ? "" : "-1";
  const body = new FormData();
  body.append("file", file, `${memberId ?? crypto.randomUUID()}${tail}.${ext}`);
  const res = await fetch("/api/assets?folderUrl=member-icons", { method: "POST", body });
  if (!res.ok) throw new Error(`${res.status}`);
  const [uploaded] = (await res.json()) as { url: string }[];
  return `${uploaded.url}?${Date.now()}`;
}

// platform asset storage (the Assets module API the AngularJS asset-list blade used)
export interface AssetEntry {
  type: "folder" | "blob";
  name: string;
  url: string;
  relativeUrl?: string;
  contentType?: string;
  size?: number;
  modifiedDate?: string;
}

export const assets = {
  list: (folderUrl: string, keyword?: string) => {
    const search = keyword ? `&keyword=${encodeURIComponent(keyword)}` : "";
    return getJson<{ results?: AssetEntry[] }>(`/api/assets?folderUrl=${encodeURIComponent(folderUrl)}${search}`).then(
      (r) => r.results ?? [],
    );
  },
  // one request per file (the endpoint keeps only the first file of a multipart body), in parallel
  async upload(folderUrl: string, files: File[], overwrite = false) {
    await Promise.all(
      files.map(async (file) => {
        const body = new FormData();
        body.append("file", file, file.name);
        const res = await fetch(
          `/api/assets?folderUrl=${encodeURIComponent(folderUrl)}${overwrite ? "&forceFileOverwrite=true" : ""}`,
          { method: "POST", body },
        );
        if (!res.ok) throw new Error(`${res.status} ${await res.text()}`);
      }),
    );
  },
  async createFolder(parentUrl: string, name: string) {
    const res = await fetch("/api/assets/folder", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, parentUrl }),
    });
    if (!res.ok) throw new Error(`${res.status} ${await res.text()}`);
  },
  async remove(urls: string[]) {
    const query = urls.map((u) => `urls=${encodeURIComponent(u)}`).join("&");
    const res = await fetch(`/api/assets?${query}`, { method: "DELETE" });
    if (!res.ok) throw new Error(`${res.status}`);
  },
};

export interface Store {
  id: string;
  name: string;
  languages?: string[];
  defaultLanguage?: string;
}

/** VcSelect options function over the Store module's search API */
export async function searchStores(keyword?: string, skip?: number, ids?: string[]) {
  const res = await fetch("/api/stores/search", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({ keyword, objectIds: ids, skip: skip ?? 0, take: 20 }),
  });
  const result = (await res.json()) as { results?: Store[]; totalCount?: number };
  return { results: result.results ?? [], totalCount: result.totalCount ?? 0 };
}
