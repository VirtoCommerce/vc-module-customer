<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.ACCOUNT_LINK.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="users"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :is-row-selectable="(user: ApplicationUser) => user.memberId !== param"
      :state-key="'CUSTOMER_ACCOUNT_LINK'"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="userName"
        :title="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.USER_NAME')"
        always-visible
      />
      <VcColumn
        id="email"
        :title="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.EMAIL')"
      />
      <VcColumn
        id="userType"
        :title="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.TYPE')"
      />
      <VcColumn
        id="memberId"
        :title="$t('CUSTOMER.PAGES.ACCOUNT_LINK.LINKED')"
      >
        <template #body="{ data: user }">
          <VcIcon
            v-if="user.memberId"
            icon="lucide-link"
            size="s"
          />
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useBlade,
  useDataTablePagination,
  useFunctions,
  usePopup,
  useTableSearch,
  type ApplicationUser,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { customerApi, securityApi } from "../api";

defineBlade({
  url: "/customer-account-link",
  name: "CustomerAccountLink",
  permissions: ["platform:security:update"],
});

// param: member id. Links existing platform accounts to the member (AngularJS pick-security-account)
const { param, callParent, closeSelf, setError } = useBlade();
const { t } = useI18n({ useScope: "global" });
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();

const PAGE_SIZE = 20;
const users = ref<ApplicationUser[]>([]);
const totalCount = ref(0);
const selection = ref<ApplicationUser[]>([]);
const loading = ref(false);
const { searchValue } = useTableSearch({ stateKey: "CUSTOMER_ACCOUNT_LINK" });
const pagination = useDataTablePagination({ stateKey: "CUSTOMER_ACCOUNT_LINK", pageSize: PAGE_SIZE, totalCount });

async function load() {
  loading.value = true;
  try {
    const result = await (
      await securityApi()
    ).searchUsers({ keyword: searchValue.value || undefined, skip: pagination.skip, take: PAGE_SIZE });
    users.value = result.results ?? [];
    totalCount.value = result.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}

load();
watch(searchValue, () => pagination.setPage(1));
watch([searchValue, () => pagination.skip], debounce(load, 300));

async function link() {
  // accounts already linked to another member move to this one only after a confirmation
  const relinked = selection.value.filter((u) => u.memberId && u.memberId !== param.value);
  if (relinked.length) {
    const members = await (await customerApi()).getMembersByIds(relinked.map((u) => u.memberId!));
    const names = members.map((m) => m.name).join(", ");
    if (!(await showConfirmation(t("CUSTOMER.ALERTS.RELINK_ACCOUNTS", { names })))) return;
  }
  loading.value = true;
  try {
    const client = await securityApi();
    for (const user of selection.value) {
      const result = await client.update({ ...user, memberId: param.value });
      if (!result.succeeded) {
        setError(result.errors?.join(", "));
        return;
      }
    }
    await callParent("reload");
    await closeSelf();
  } finally {
    loading.value = false;
  }
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "link",
    title: t("CUSTOMER.PAGES.ACCOUNTS.TOOLBAR.LINK"),
    icon: "lucide-link",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => link(),
  },
  { id: "refresh", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);
</script>
