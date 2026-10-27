import { computed, onMounted, ref } from "vue";
import {
  defineBladeContext,
  DynamicPropertiesClient,
  useApiClient,
  useBlade,
  useBladeForm,
  useBladeWidgets,
  usePermissions,
  usePopup,
  type ApplicationUser,
  type DynamicProperty,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { useI18n } from "vue-i18n";
import { customerApi, fullTypeName, getSettingDefault, membershipApi, MEMBER_TYPES, type MemberType } from "../api";
import type {
  Contact,
  CustomerAddress,
  DynamicObjectProperty,
  Employee,
  Organization,
  SeoInfo,
  Vendor,
} from "../../api_client/virtocommerce.customer";

export type MemberModel = Contact & Organization & Employee & Vendor;

// The four member details blades (Contact, Organization, Employee, Vendor) share load/save/toolbar/widgets;
// what used to be memberDetailController + the per-type controllers in the AngularJS module.
export function useMemberBlade(memberType: MemberType) {
  const { t } = useI18n({ useScope: "global" });
  const { param, options, openBlade, callParent, closeChildren, exposeToChildren, replaceWith } = useBlade<{
    parentId?: string;
  }>();
  const { hasAccess } = usePermissions();
  const { showInfo } = usePopup();

  const item = ref<MemberModel>({});
  const loading = ref(false);
  const membershipsCount = ref(0);
  const isNew = computed(() => !param.value);

  const form = useBladeForm({
    data: item,
    closeConfirmMessage: computed(() => t("CUSTOMER.ALERTS.CLOSE_CONFIRMATION")),
  });

  const title = computed(() =>
    isNew.value ? t(`CUSTOMER.PAGES.DETAILS.NEW_${memberType.toUpperCase()}`) : (item.value.name ?? ""),
  );

  // the dynamic properties registered for the type: read once per blade, not again on every save
  let definitions: Promise<DynamicProperty[]> | undefined;
  const loadDefinitions = () =>
    (definitions ??= useApiClient(DynamicPropertiesClient)
      .getApiClient()
      .then((client) => client.searchDynamicProperties({ objectType: fullTypeName(memberType), take: 1000 }))
      .then((r) => r.results ?? [])
      .catch((e) => {
        definitions = undefined;
        return Promise.reject(e);
      }));

  async function newMember(): Promise<MemberModel> {
    const parentId = options.value?.parentId;
    const status = await getSettingDefault(MEMBER_TYPES[memberType].statusSetting);
    return {
      memberType,
      status,
      dynamicProperties: [],
      ...(memberType === "Organization" && { parentId }),
      ...((memberType === "Contact" || memberType === "Employee") && { organizations: parentId ? [parentId] : [] }),
      ...(memberType === "Employee" && { isActive: true }),
      ...(memberType === "Vendor" && { seoInfos: [] }),
    };
  }

  async function load() {
    loading.value = true;
    try {
      const [member, defs] = await Promise.all([
        isNew.value ? newMember() : customerApi().then((client) => client.getMemberById(param.value!) as Promise<MemberModel>),
        hasAccess("platform:dynamic_properties:read") ? loadDefinitions() : undefined,
      ]);
      // the API returns DateTime.MinValue for an empty birthday (same fix as the AngularJS members resource)
      if (member.birthDate && new Date(member.birthDate).getUTCFullYear() <= 1) member.birthDate = undefined;
      member.addresses ??= [];
      member.phones ??= [];
      member.emails ??= [];
      member.groups ??= [];
      // all dynamic properties registered for the type, with this member's values (like propertyValue-list.js)
      if (defs)
        member.dynamicProperties = defs.map((def) => ({
          ...def,
          objectId: member.id,
          values: member.dynamicProperties?.find((x) => x.name === def.name)?.values ?? [],
        })) as DynamicObjectProperty[];
      item.value = member;
      form.setBaseline();
      await loadMembershipsCount();
    } finally {
      loading.value = false;
    }
  }

  const accounts = computed(() => item.value.securityAccounts ?? []);

  async function loadMembershipsCount() {
    const userId = accounts.value[0]?.id;
    if (memberType !== "Contact" || !userId) return;
    membershipsCount.value = (await (await membershipApi()).search({ userId, take: 0 })).totalCount ?? 0;
  }

  // accounts changed in the accounts blade, which hands over the member's accounts it just loaded: take them
  // without discarding unsaved edits
  async function refreshAccounts(fresh: unknown) {
    const wasModified = form.isModified.value;
    item.value.securityAccounts = fresh as ApplicationUser[] as MemberModel["securityAccounts"];
    if (!wasModified) form.setBaseline();
    await loadMembershipsCount();
  }

  async function save() {
    // address and SEO blades edit by list index, which the reload below reorders (legacy closeChildrenBlades)
    await closeChildren();
    loading.value = true;
    try {
      const client = await customerApi();
      // the server takes a person's display name from fullName
      // (and the Employee mapper only keeps a name the client sends)
      if (memberType === "Contact" || memberType === "Employee") {
        item.value.fullName ||= [item.value.firstName, item.value.lastName].filter(Boolean).join(" ");
        item.value.name = item.value.fullName;
      }
      if (isNew.value) {
        const created = await client.createMember(item.value);
        form.setBaseline();
        callParent("reload");
        await replaceWith({ name: MEMBER_TYPES[memberType].blade, param: created.id });
        return;
      }
      await client.updateMember(item.value);
      callParent("reload");
    } finally {
      loading.value = false;
    }
    await load();
  }

  const toolbar = ref<IBladeToolbar[]>([
    {
      id: "save",
      title: computed(() => (isNew.value ? t("CUSTOMER.PAGES.DETAILS.TOOLBAR.CREATE") : t("CUSTOMER.PAGES.DETAILS.TOOLBAR.SAVE"))),
      icon: "lucide-save",
      permissions: isNew.value ? "customer:create" : "customer:update",
      disabled: computed(() => !form.canSave.value),
      clickHandler: () => save(),
    },
    {
      id: "reset",
      title: t("CUSTOMER.PAGES.DETAILS.TOOLBAR.RESET"),
      icon: "lucide-undo-2",
      isVisible: computed(() => !isNew.value),
      disabled: computed(() => !form.isModified.value),
      clickHandler: () => form.revert(),
    },
  ]);

  const hasAccounts = memberType !== "Organization";
  useBladeWidgets([
    {
      id: "CustomerAccounts",
      icon: "lucide-key-round",
      title: "CUSTOMER.WIDGETS.ACCOUNTS",
      badge: computed(() => accounts.value.length),
      isVisible: computed(() => hasAccounts && !isNew.value && hasAccess("platform:security:read")),
      onClick: () => openBlade({ name: "CustomerAccounts", param: item.value.id, options: { memberName: item.value.name } }),
    },
    {
      id: "CustomerOrgMemberships",
      icon: "lucide-network",
      title: "CUSTOMER.WIDGETS.MEMBERSHIPS",
      badge: membershipsCount,
      isVisible: computed(() => memberType === "Contact" && !isNew.value && hasAccess("platform:security:read")),
      onClick: () => {
        const account = accounts.value[0];
        if (!account) return showInfo(t("CUSTOMER.ALERTS.NO_ACCOUNT_MEMBERSHIPS"));
        openBlade({
          name: "CustomerOrgMemberships",
          param: account.id,
          options: { storeId: account.storeId, memberName: item.value.name },
        });
      },
    },
    {
      id: "CustomerVendorSeo",
      icon: "lucide-globe",
      title: "CUSTOMER.WIDGETS.SEO",
      badge: computed(() => item.value.seoInfos?.length ?? 0),
      isVisible: computed(() => memberType === "Vendor" && !isNew.value),
      onClick: () => openBlade({ name: "CustomerSeoList" }),
    },
    {
      id: "CustomerOrganizationAssets",
      icon: "lucide-folder-open",
      title: "CUSTOMER.WIDGETS.ASSETS",
      isVisible: computed(() => memberType === "Organization" && !isNew.value && hasAccess("platform:asset:read")),
      onClick: () => openBlade({ name: "CustomerOrganizationAssets", param: item.value.id, options: { name: item.value.name } }),
    },
  ]);

  // vendor SEO records, edited in the SEO blades and saved with the vendor (legacy seo-list/seo-detail)
  function saveSeo(args: unknown) {
    const { seo, index } = args as { seo: SeoInfo; index: number };
    const list = (item.value.seoInfos ??= []);
    if (index >= 0) list.splice(index, 1, seo);
    else list.push(seo);
  }
  function deleteSeo(args: unknown) {
    const indexes = args as number[];
    item.value.seoInfos = (item.value.seoInfos ?? []).filter((_, i) => !indexes.includes(i));
  }

  // addresses are edited in a child blade and saved with the member, as in the AngularJS address list
  const sameType = (a: CustomerAddress, type?: string) => a.addressType === type;
  function saveAddress(args: unknown) {
    const { address, index } = args as { address: CustomerAddress; index: number };
    const list = item.value.addresses!;
    const original = index >= 0 ? list[index] : undefined;
    const others = list.filter((_, i) => i !== index);
    if (original && original.addressType !== address.addressType) address.isDefault = false;
    // the first address of a type becomes its default; a new default clears the others of that type
    if (!others.some((a) => sameType(a, address.addressType))) address.isDefault = true;
    if (address.isDefault) others.filter((a) => sameType(a, address.addressType)).forEach((a) => (a.isDefault = false));
    if (address.addressType === "BillingAndShipping") address.isDefault = false;
    // the type that lost its only default gets a new one
    if (original?.isDefault && original.addressType !== address.addressType) {
      const next = others.find((a) => sameType(a, original.addressType));
      if (next && original.addressType !== "BillingAndShipping") next.isDefault = true;
    }
    if (index >= 0) list.splice(index, 1, address);
    else list.push(address);
  }

  function deleteAddress(args: unknown) {
    const index = args as number;
    const list = item.value.addresses!;
    const [removed] = list.splice(index, 1);
    const next = list.find((a) => sameType(a, removed?.addressType));
    if (removed?.isDefault && next && removed.addressType !== "BillingAndShipping") next.isDefault = true;
  }

  exposeToChildren({
    saveAddress,
    deleteAddress,
    refreshAccounts,
    setMembershipsCount: (count: unknown) => (membershipsCount.value = Number(count) || 0),
    reload: load,
    seoInfos: () => item.value.seoInfos ?? [],
    saveSeo,
    deleteSeo,
  });

  // the member, for widgets other modules add to these blades (registerExternalWidget targetBlades)
  defineBladeContext({ item });

  onMounted(load);

  return { item, title, toolbar, loading, isNew };
}
