<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.LIST.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <div
      v-if="path.length"
      class="customer-list__crumbs"
    >
      <VcBreadcrumbs :items="breadcrumbs" />
    </div>
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="data"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :state-key="'CUSTOMER_MEMBERS'"
      :active-item-id="activeChildParam"
      :show-all-columns="!activeChildParam"
      :global-filters="globalFilters"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      :empty-state="{ icon: 'lucide-contact', title: $t('CUSTOMER.PAGES.LIST.EMPTY') }"
      @row-click="onRowClick"
      @pagination-click="pagination.goToPage"
      @filter="onFilter"
    >
      <VcColumn
        id="name"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.NAME')"
        sortable
        always-visible
      >
        <template #body="{ data: member }">
          <div class="customer-list__name">
            <VcImage
              v-if="member.iconUrl"
              :src="member.iconUrl"
              size="xxs"
              rounded
              class="customer-list__avatar"
            />
            <VcIcon
              v-else
              :icon="typeIcon(member.memberType)"
              size="m"
              class="customer-list__icon"
            />
            <span>{{ member.name }}</span>
            <VcIcon
              v-if="member.memberType === 'Organization'"
              icon="lucide-chevron-right"
              size="s"
              class="customer-list__drill"
            />
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="memberType"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.TYPE')"
        sortable
      >
        <template #body="{ data: member }">{{ typeTitle(member.memberType) }}</template>
      </VcColumn>
      <VcColumn
        id="emails"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.EMAIL')"
      >
        <template #body="{ data: member }">{{ member.emails?.[0] }}</template>
      </VcColumn>
      <VcColumn
        id="status"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.STATUS')"
      >
        <template #body="{ data: member }">
          <div
            v-if="member.status"
            class="dash-inline"
          >
            <VcStatus :variant="statusVariant(member.status)">{{ member.status }}</VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="outerId"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.OUTER_ID')"
        :visible="false"
      />
      <VcColumn
        id="createdDate"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.CREATED')"
        type="date"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
/**
 * Extension point "customer-members-toolbar": other modules add toolbar commands to this list
 * (legacy: toolbarService.register(..., "virtoCommerce.customerModule.memberListController"), e.g. CSV import/export).
 *
 *   useExtensionPoint("customer-members-toolbar").add({
 *     id: "my-command",
 *     priority: 10,                               // order among the added commands
 *     component: markRaw({ render: () => null }), // required by the framework type, not rendered
 *     meta: {
 *       title: "MYMODULE.COMMAND",                // i18n key or text
 *       icon: "lucide-download",
 *       permissions: "customer:simpleExport",     // optional, string or string[] (any of)
 *       isVisible: (ctx) => true,                 // optional
 *       disabled: (ctx) => !ctx.selection.length, // optional
 *       onClick: (ctx) => ctx.openBlade({ name: "MyBlade", options: { organization: ctx.organization } }),
 *     } satisfies CustomerMembersToolbarCommand,
 *   });
 *
 * ctx (CustomerMembersToolbarContext):
 *   organization  the organization the list is drilled into (Member), undefined at the root
 *   selection     the selected rows (Member: id, name, memberType...)
 *   keyword       the list's search keyword (search text + filter tokens), undefined when empty
 *   totalCount    the number of rows in the list (all pages)
 *   openBlade     opens a blade as a child of this list
 *   reload        reloads the list
 * Hidden when the list is opened read-only.
 */
import {
  defineExtensionPoint,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  usePermissions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcBreadcrumbs, VcColumn, VcDataTable, VcIcon, VcImage, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { customerApi, membershipApi, MEMBER_TYPES, type MemberType } from "../api";
import type { Member, OrganizationMembership } from "../../api_client/virtocommerce.customer";

export interface CustomerMembersToolbarContext {
  organization?: Member;
  selection: Member[];
  keyword?: string;
  totalCount: number;
  openBlade: ReturnType<typeof useBlade>["openBlade"];
  reload: () => Promise<void>;
}
export interface CustomerMembersToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: CustomerMembersToolbarContext) => boolean;
  disabled?: (ctx: CustomerMembersToolbarContext) => boolean;
  onClick: (ctx: CustomerMembersToolbarContext) => unknown;
}
const extensions = defineExtensionPoint<CustomerMembersToolbarCommand>("customer-members-toolbar", {
  description: "Toolbar commands of the customer members list",
});

defineBlade({
  url: "/customers",
  name: "CustomerMembersList",
  isWorkspace: true,
  permissions: ["customer:access"],
  menuItem: {
    title: "CUSTOMER.MENU.TITLE",
    icon: "lucide-contact",
    priority: 20,
  },
});

// options.groups / options.readOnly: opened by other modules (e.g. Marketing user-group conditions), like legacy readOnlyList
const { openBlade, closeChildren, exposeToChildren, activeChildParam, options } = useBlade<{
  groups?: string[];
  readOnly?: boolean;
}>();
const { t } = useI18n({ useScope: "global" });
const { debounce } = useFunctions();
const { hasAccess } = usePermissions();
const { showConfirmation, showError, showInfo } = usePopup();

const PAGE_SIZE = 20;
const readOnly = computed(() => !!options.value?.readOnly);

const data = ref<Member[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const selection = ref<Member[]>([]);
// organization drill-down: the stack of organizations the user went into (legacy opened a list blade per level)
const path = ref<Member[]>([]);
const currentOrg = computed(() => path.value[path.value.length - 1]);

const { sortField, sortOrder, sortExpression } = useDataTableSort({
  stateKey: "CUSTOMER_MEMBERS",
  initialField: "memberType",
  initialDirection: "DESC",
});
const { searchValue } = useTableSearch({ stateKey: "CUSTOMER_MEMBERS" });
const pagination = useDataTablePagination({ stateKey: "CUSTOMER_MEMBERS", pageSize: PAGE_SIZE, totalCount });

const typeIcon = (type?: string) => MEMBER_TYPES[type as MemberType]?.icon ?? "lucide-user";
const typeTitle = (type?: string) => (type ? t(`CUSTOMER.TYPES.${type.toUpperCase()}`) : "");
const statusVariant = (status: string) =>
  (({ Approved: "success", Rejected: "danger", Deleted: "danger" }) as Record<string, "success" | "danger">)[status] ??
  "info";

// filters, sent as index keyword tokens exactly like the AngularJS member list
const filters = ref<Record<string, string | undefined>>({});
const globalFilters = computed(() => [
  {
    id: "memberType",
    label: t("CUSTOMER.PAGES.LIST.FILTERS.TYPE"),
    filter: { options: Object.keys(MEMBER_TYPES).map((x) => ({ value: x, label: typeTitle(x) })) },
  },
  {
    id: "created",
    label: t("CUSTOMER.PAGES.LIST.FILTERS.CREATED"),
    filter: { range: ["createdStart", "createdEnd"] as [string, string] },
  },
  {
    id: "modified",
    label: t("CUSTOMER.PAGES.LIST.FILTERS.MODIFIED"),
    filter: { range: ["modifiedStart", "modifiedEnd"] as [string, string] },
  },
]);

function onFilter(event: { filters: Record<string, unknown> }) {
  filters.value = Object.fromEntries(
    Object.entries(event.filters).map(([k, v]) => [k, (Array.isArray(v) ? v[0] : v) || undefined]),
  ) as Record<string, string | undefined>;
  // a page change loads through the watcher below
  if (pagination.skip) pagination.setPage(1);
  else load();
}

const day = (d: string) => new Date(d).toISOString().slice(0, 10);
function rangeToken(field: string, start?: string, end?: string) {
  if (!start && !end) return "";
  const from = start ? `"${day(start)}T00:00:00Z"` : '""';
  const to = end ? `"${day(end)}T23:59:59Z"` : '""';
  return `${field}:[${from} TO ${to}]`;
}

function keyword() {
  const f = filters.value;
  return [
    searchValue.value,
    f.memberType && `membertype:${f.memberType}`,
    rangeToken("createddate", f.createdStart, f.createdEnd),
    rangeToken("modifieddate", f.modifiedStart, f.modifiedEnd),
  ]
    .filter(Boolean)
    .join(" ");
}

async function load() {
  loading.value = true;
  try {
    const kw = keyword();
    const sort = sortExpression.value?.startsWith("memberType")
      ? `${sortExpression.value};name:asc`
      : sortExpression.value;
    const result = await (
      await customerApi()
    ).searchMember({
      memberId: currentOrg.value?.id,
      keyword: kw || undefined,
      deepSearch: !!kw,
      groups: options.value?.groups,
      sort,
      skip: pagination.skip,
      take: PAGE_SIZE,
    });
    data.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

const breadcrumbs = computed(() => [
  { id: "", title: t("CUSTOMER.PAGES.LIST.ALL"), icon: "lucide-contact", clickHandler: () => goTo(0) },
  ...path.value.map((org, i) => ({ id: org.id!, title: org.name, clickHandler: () => goTo(i + 1) })),
]);

// each level starts unfiltered by keyword, like the AngularJS list blade opened per level; a cleared search or a
// page change loads through the watchers below
function goTo(depth: number) {
  path.value = path.value.slice(0, depth);
  selection.value = [];
  if (searchValue.value) searchValue.value = "";
  else if (pagination.skip) pagination.setPage(1);
  else load();
}

function openDetails(member: Member) {
  const blade = MEMBER_TYPES[member.memberType as MemberType]?.blade;
  // legacy: a type without a details blade can't be edited
  if (!blade) return showError(t("CUSTOMER.ALERTS.UNKNOWN_MEMBER_TYPE", { memberType: member.memberType }));
  openBlade({ name: blade, param: member.id });
}

function onRowClick(event: { data: Member }) {
  if (event.data.memberType === "Organization") {
    path.value = [...path.value, event.data];
    goTo(path.value.length);
  } else {
    openDetails(event.data);
  }
}

const copy = (text?: string) => text && navigator.clipboard.writeText(text);

const rowActions = (member: Member) => [
  { id: "manage", title: t("CUSTOMER.PAGES.LIST.ACTIONS.MANAGE"), icon: "lucide-pencil", type: "info" as const, clickHandler: () => openDetails(member) },
  { id: "copyId", title: t("CUSTOMER.PAGES.LIST.ACTIONS.COPY_ID"), icon: "lucide-copy", type: "info" as const, clickHandler: () => copy(member.id) },
  { id: "copyName", title: t("CUSTOMER.PAGES.LIST.ACTIONS.COPY_NAME"), icon: "lucide-copy", type: "info" as const, clickHandler: () => copy(member.name) },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    type: "danger" as const,
    variant: "danger" as const,
    hidden: readOnly.value || !hasAccess("customer:delete"),
    clickHandler: () => remove([member]),
  },
];

async function remove(members: Member[]) {
  const counts = members.reduce<Record<string, number>>((acc, m) => {
    acc[m.memberType!] = (acc[m.memberType!] ?? 0) + 1;
    return acc;
  }, {});
  const what = Object.entries(counts)
    .map(([type, count]) => t(`CUSTOMER.ALERTS.DELETE_${type.toUpperCase()}`, { count }))
    .join(", ");
  if (!(await showConfirmation(t("CUSTOMER.ALERTS.DELETE_CONFIRMATION", { what })))) return;
  // an open details blade of a deleted member would re-create it on Save (legacy closeChildrenBlades)
  await closeChildren();
  loading.value = true;
  try {
    await (await customerApi()).deleteMembers(members.map((x) => x.id!));
    selection.value = [];
  } finally {
    await load();
  }
}

// lock / unlock / change role of the selected member inside the current organization (legacy org commands)
async function selectedMembership() {
  const member = await (await customerApi()).getMemberById(selection.value[0].id!);
  const account = (member as { securityAccounts?: { id?: string }[] }).securityAccounts?.[0];
  if (!account?.id) {
    showInfo(t("CUSTOMER.ALERTS.NO_ACCOUNT"));
    return undefined;
  }
  try {
    return await (await membershipApi()).getByUserAndOrg(account.id, currentOrg.value!.id!);
  } catch {
    showInfo(t("CUSTOMER.ALERTS.NO_MEMBERSHIP"));
    return undefined;
  }
}

const inOrgSingle = computed(() => !!currentOrg.value && selection.value.length === 1);
const orgCommand = (id: string, title: string, icon: string, run: (membership: OrganizationMembership) => unknown): IBladeToolbar => ({
  id,
  title: t(title),
  icon,
  permissions: "platform:security:update",
  isVisible: computed(() => !!currentOrg.value && !readOnly.value),
  disabled: computed(() => !inOrgSingle.value),
  async clickHandler() {
    const membership = await selectedMembership();
    if (membership) await run(membership);
  },
});

const ownToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "customer:create",
    isVisible: computed(() => !readOnly.value),
    clickHandler: () =>
      openBlade({
        name: "CustomerMemberAdd",
        options: { parentId: currentOrg.value?.id, parentType: currentOrg.value?.memberType },
      }),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "customer:delete",
    isVisible: computed(() => !readOnly.value),
    disabled: computed(() => !selection.value.length),
    clickHandler: () => remove(selection.value),
  },
  {
    id: "invite",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.INVITE"),
    icon: "lucide-send",
    permissions: "customer:invite",
    isVisible: computed(() => !readOnly.value),
    clickHandler: () =>
      openBlade({
        name: "CustomerInvite",
        options: { organizationId: currentOrg.value?.id },
      }),
  },
  orgCommand("lock", "CUSTOMER.PAGES.LIST.TOOLBAR.LOCK", "lucide-lock", async (m) => {
    await (await membershipApi()).lock(m.id!, {});
    await load();
  }),
  orgCommand("unlock", "CUSTOMER.PAGES.LIST.TOOLBAR.UNLOCK", "lucide-lock-open", async (m) => {
    await (await membershipApi()).unlock(m.id!);
    await load();
  }),
  orgCommand("role", "CUSTOMER.PAGES.LIST.TOOLBAR.CHANGE_ROLE", "lucide-user-cog", (m) =>
    openBlade({ name: "CustomerOrgMembershipDetails", param: m.id, options: { userId: m.userId } }),
  ),
]);

const toolbarContext = (): CustomerMembersToolbarContext => ({
  organization: currentOrg.value,
  selection: selection.value,
  keyword: keyword() || undefined,
  totalCount: totalCount.value,
  openBlade,
  reload: load,
});

const bladeToolbar = computed<IBladeToolbar[]>(() => [
  ...ownToolbar.value,
  // commands other modules add (extension point "customer-members-toolbar")
  ...(readOnly.value ? [] : extensions.components.value)
    .filter((x) => x.meta && (!x.meta.isVisible || x.meta.isVisible(toolbarContext())))
    .map((x) => ({
      id: x.id,
      title: t(x.meta!.title),
      icon: x.meta!.icon,
      permissions: x.meta!.permissions,
      disabled: !!x.meta!.disabled?.(toolbarContext()),
      clickHandler: () => x.meta!.onClick(toolbarContext()),
    })),
]);

exposeToChildren({ reload: load });
</script>

<style lang="scss">
.customer-list__crumbs {
  padding: 12px 16px 0;
}

.customer-list__name {
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--color-neutral-900);
}

.customer-list__icon {
  color: var(--color-neutral-500);
  flex-shrink: 0;
}

.customer-list__avatar {
  flex-shrink: 0;
}

.customer-list__drill {
  color: var(--color-neutral-400);
}
</style>
