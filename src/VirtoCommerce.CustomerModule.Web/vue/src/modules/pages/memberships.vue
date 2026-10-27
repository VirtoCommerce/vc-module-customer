<template>
  <VcBlade
    :title="$t('CUSTOMER.WIDGETS.MEMBERSHIPS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :active-item-id="activeChildParam"
      :state-key="'CUSTOMER_MEMBERSHIPS'"
      :empty-state="{ icon: 'lucide-network', title: $t('CUSTOMER.PAGES.MEMBERSHIPS.EMPTY') }"
      @row-click="(e: { data: OrganizationMembership }) => open(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="organizationName"
        :title="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.ORGANIZATION')"
        always-visible
      >
        <template #body="{ data: m }">{{ m.organizationId ? m.organizationName : $t("CUSTOMER.PAGES.MEMBERSHIPS.GLOBAL") }}</template>
      </VcColumn>
      <VcColumn
        id="roles"
        :title="$t('CUSTOMER.FIELDS.ROLES')"
      >
        <template #body="{ data: m }">{{ (m.roles ?? []).map((r: { roleName?: string }) => r.roleName).join(", ") || "—" }}</template>
      </VcColumn>
      <VcColumn
        id="status"
        :title="$t('CUSTOMER.FIELDS.STATUS')"
      />
      <VcColumn
        id="isLocked"
        :title="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.LOCKED')"
      >
        <template #body="{ data: m }">
          <VcIcon
            v-if="m.isLocked"
            icon="lucide-lock"
            size="s"
          />
        </template>
      </VcColumn>
      <VcColumn
        id="lockoutEnd"
        :title="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.LOCKOUT_END')"
        type="date"
        :visible="false"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { membershipApi } from "../api";
import type { OrganizationMembership } from "../../api_client/virtocommerce.customer";

defineBlade({
  url: "/customer-memberships",
  name: "CustomerOrgMemberships",
  permissions: ["platform:security:read"],
});

// param: user (security account) id; options.storeId narrows the roles whitelist in the details blade
const { param, options, openBlade, closeChildren, callParent, exposeToChildren, activeChildParam } = useBlade<{
  storeId?: string;
}>();
const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();

const PAGE_SIZE = 20;
const items = ref<OrganizationMembership[]>([]);
const totalCount = ref(0);
const selection = ref<OrganizationMembership[]>([]);
const loading = ref(false);
const pagination = useDataTablePagination({ stateKey: "CUSTOMER_MEMBERSHIPS", pageSize: PAGE_SIZE, totalCount });

async function load() {
  loading.value = true;
  try {
    const result = await (await membershipApi()).search({ userId: param.value, skip: pagination.skip, take: PAGE_SIZE });
    items.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
    selection.value = [];
  } finally {
    loading.value = false;
  }
}

// reload here and the member blade's Memberships badge (this list is its first account's)
async function reload() {
  await load();
  await callParent("setMembershipsCount", totalCount.value);
}

load();
watch(() => pagination.skip, load);
exposeToChildren({ reload, usedOrganizations: () => items.value.map((x) => x.organizationId).filter(Boolean) });

const open = (m: OrganizationMembership) =>
  openBlade({ name: "CustomerOrgMembershipDetails", param: m.id, options: { userId: param.value, storeId: options.value?.storeId } });

// the global (organization-less) membership can't be deleted
const deletable = computed(() => selection.value.filter((x) => x.organizationId));

const toolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "customer:update",
    clickHandler: () =>
      openBlade({ name: "CustomerOrgMembershipDetails", options: { userId: param.value, storeId: options.value?.storeId } }),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "customer:delete",
    disabled: computed(() => !deletable.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("CUSTOMER.ALERTS.DELETE_MEMBERSHIPS", { count: deletable.value.length })))) return;
      await closeChildren();
      await (await membershipApi()).delete(deletable.value.map((x) => x.id!));
      await reload();
    },
  },
]);
</script>
