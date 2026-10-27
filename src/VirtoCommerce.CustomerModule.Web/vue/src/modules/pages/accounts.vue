<template>
  <VcBlade
    :title="$t('CUSTOMER.WIDGETS.ACCOUNTS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="accounts"
      :state-key="'CUSTOMER_ACCOUNTS'"
      :active-item-id="activeChildParam"
      :empty-state="{ icon: 'lucide-key-round', title: $t('CUSTOMER.PAGES.ACCOUNTS.EMPTY') }"
      @row-click="(e: { data: ApplicationUser }) => openAccount(e.data)"
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
        id="status"
        :title="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.STATUS')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type ApplicationUser, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { securityApi } from "../api";

defineBlade({
  url: "/customer-accounts",
  name: "CustomerAccounts",
  permissions: ["platform:security:read"],
});

// param: member id. Security accounts linked to the member (AngularJS customer-accounts-list)
const { param, openBlade, closeChildren, callParent, exposeToChildren, setError, activeChildParam } = useBlade();
const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();

const accounts = ref<ApplicationUser[]>([]);
const selection = ref<ApplicationUser[]>([]);
const loading = ref(false);

async function load() {
  loading.value = true;
  try {
    const result = await (await securityApi()).searchUsers({ memberId: param.value, take: 100 });
    accounts.value = result.results ?? [];
    selection.value = [];
  } finally {
    loading.value = false;
  }
}

// reload here and in the member blade (its Accounts badge and organization memberships)
async function reload() {
  await load();
  // the member blade may already be gone (e.g. the whole stack was replaced while a child was open)
  await callParent("refreshAccounts", accounts.value).catch(() => undefined);
}

onMounted(load);

// the platform plugin's user details; fromContact hides its "Show contact" widget (we came from the contact)
const openAccount = (user: ApplicationUser) =>
  openBlade({ name: "PlatformSecurityUserDetails", param: user.id, options: { fromContact: true }, onClose: () => reload() });
exposeToChildren({ reload });

async function unlink() {
  if (!(await showConfirmation(t("CUSTOMER.ALERTS.UNLINK_ACCOUNTS", { count: selection.value.length })))) return;
  await closeChildren();
  const client = await securityApi();
  // in parallel, as the AngularJS blade did
  const results = await Promise.all(selection.value.map((account) => client.update({ ...account, memberId: undefined })));
  const errors = results.flatMap((r) => (r.succeeded ? [] : (r.errors ?? [])));
  if (errors.length) setError(errors.join(", "));
  await reload();
}

async function remove() {
  if (!(await showConfirmation(t("CUSTOMER.ALERTS.DELETE_ACCOUNTS", { count: selection.value.length })))) return;
  await closeChildren();
  await (await securityApi()).delete(selection.value.map((x) => x.userName!));
  await reload();
}

const toolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("CUSTOMER.PAGES.ACCOUNTS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "platform:security:create",
    clickHandler: () => openBlade({ name: "CustomerAccountCreate", param: param.value }),
  },
  {
    id: "link",
    title: t("CUSTOMER.PAGES.ACCOUNTS.TOOLBAR.LINK"),
    icon: "lucide-link",
    permissions: "platform:security:update",
    clickHandler: () => openBlade({ name: "CustomerAccountLink", param: param.value }),
  },
  {
    id: "unlink",
    title: t("CUSTOMER.PAGES.ACCOUNTS.TOOLBAR.UNLINK"),
    icon: "lucide-unlink",
    permissions: "platform:security:update",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => unlink(),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "platform:security:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => remove(),
  },
]);
</script>
