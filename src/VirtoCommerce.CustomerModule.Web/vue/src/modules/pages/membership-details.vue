<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="customer-form tw-space-y-4">
        <Field
          v-if="isNew"
          v-slot="{ errorMessage, handleChange, errors }"
          name="organizationId"
          rules="required"
          :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.ORGANIZATION')"
          :model-value="item.organizationId"
        >
          <VcSelect
            v-model="item.organizationId"
            :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.ORGANIZATION')"
            :options="searchOrganizations"
            option-value="id"
            option-label="name"
            searchable
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcField
          v-else
          :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.ORGANIZATION')"
          :model-value="item.organizationId ? item.organizationName : $t('CUSTOMER.PAGES.MEMBERSHIPS.GLOBAL')"
        />
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="roles"
          rules="required"
          :label="$t('CUSTOMER.FIELDS.ROLES')"
          :model-value="roleIds"
        >
          <VcSelect
            v-model="roleIds"
            :label="$t('CUSTOMER.FIELDS.ROLES')"
            :hint="$t('CUSTOMER.PAGES.MEMBERSHIPS.ROLES_HINT')"
            :options="searchRoles"
            option-value="id"
            option-label="name"
            multiple
            searchable
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcSelect
          v-if="!isNew"
          v-model="item.status"
          :label="$t('CUSTOMER.FIELDS.STATUS')"
          :placeholder="$t('CUSTOMER.PAGES.MEMBERSHIPS.STATUS_INHERITED')"
          :hint="$t('CUSTOMER.PAGES.MEMBERSHIPS.STATUS_HINT')"
          :options="toOptions([...statuses, item.status])"
        />
        <VcField
          v-if="!isNew"
          :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.LOCKED')"
          :model-value="item.isLocked ? $t('CUSTOMER.PAGES.MEMBERSHIPS.LOCKED') : $t('CUSTOMER.PAGES.MEMBERSHIPS.UNLOCKED')"
        />
        <VcInput
          v-if="item.isLocked"
          v-model="lockoutEnd"
          type="date"
          :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.LOCKOUT_END')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcField, VcForm, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getSettingValues, localDayToUtc, membershipApi, organizationOptions, rolesOptions, toOptions, utcDayToLocal } from "../api";
import type { OrganizationMembership } from "../../api_client/virtocommerce.customer";

defineBlade({
  url: "/customer-membership",
  name: "CustomerOrgMembershipDetails",
});

// param: membership id (none = new); options.userId: the account; options.storeId: store roles whitelist
const { param, options, callParent, replaceWith } = useBlade<{ userId?: string; storeId?: string }>();
const { t } = useI18n({ useScope: "global" });
const { showInfo } = usePopup();

const isNew = computed(() => !param.value);
const item = ref<OrganizationMembership>({ userId: options.value?.userId, roles: [], isLocked: false });
const statuses = ref<string[]>([]);
const loading = ref(false);

const form = useBladeForm({
  data: item,
  closeConfirmMessage: computed(() => t("CUSTOMER.ALERTS.CLOSE_CONFIRMATION")),
});

const title = computed(() => {
  if (isNew.value) return t("CUSTOMER.PAGES.MEMBERSHIPS.NEW");
  return item.value.organizationId ? (item.value.organizationName ?? "") : t("CUSTOMER.PAGES.MEMBERSHIPS.GLOBAL");
});

async function load() {
  loading.value = true;
  try {
    if (!isNew.value) item.value = await (await membershipApi()).getById(param.value!);
    form.setBaseline();
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  [, statuses.value] = await Promise.all([load(), getSettingValues("Customer.OrganizationMembershipStatuses")]);
});

const { search: searchRoles, toRoles } = rolesOptions(
  "Customer.MembershipRolesWhitelist",
  () => item.value.roles ?? [],
  options.value?.storeId,
);
const roleIds = computed({
  get: () => (item.value.roles ?? []).map((r) => r.roleId!),
  set: (ids: string[]) => (item.value.roles = toRoles(ids, item.value.roles)),
});

// organizations the account is not a member of yet
const usedOrganizations = async () => (await callParent<string[]>("usedOrganizations").catch((): string[] => [])) ?? [];
const searchOrganizations = organizationOptions(usedOrganizations);

const lockoutEnd = computed({
  get: () => utcDayToLocal(item.value.lockoutEnd),
  set: (value: Date | string | null) => (item.value.lockoutEnd = localDayToUtc(value)),
});

async function save() {
  loading.value = true;
  try {
    const client = await membershipApi();
    if (isNew.value) {
      if ((await usedOrganizations()).includes(item.value.organizationId!)) return showInfo(t("CUSTOMER.ALERTS.DUPLICATE_MEMBERSHIP"));
      const created = await client.create(item.value);
      form.setBaseline();
      await callParent("reload");
      await replaceWith({ name: "CustomerOrgMembershipDetails", param: created.id, options: options.value });
      return;
    }
    item.value = await client.update(item.value.id!, item.value);
    form.setBaseline();
    await callParent("reload");
  } finally {
    loading.value = false;
  }
}

async function setLock(lock: boolean) {
  loading.value = true;
  try {
    const client = await membershipApi();
    item.value = lock ? await client.lock(item.value.id!, {}) : await client.unlock(item.value.id!);
    form.setBaseline();
    await callParent("reload");
  } finally {
    loading.value = false;
  }
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => (isNew.value ? t("CUSTOMER.PAGES.DETAILS.TOOLBAR.CREATE") : t("CUSTOMER.PAGES.DETAILS.TOOLBAR.SAVE"))),
    icon: "lucide-save",
    permissions: "customer:update",
    disabled: computed(() => !form.canSave.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("CUSTOMER.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "lock",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.LOCK"),
    icon: "lucide-lock",
    permissions: "customer:update",
    isVisible: computed(() => !isNew.value && !item.value.isLocked),
    clickHandler: () => setLock(true),
  },
  {
    id: "unlock",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.UNLOCK"),
    icon: "lucide-lock-open",
    permissions: "customer:update",
    isVisible: computed(() => !isNew.value && !!item.value.isLocked),
    clickHandler: () => setLock(false),
  },
]);
</script>
