<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="customer-form tw-space-y-4">
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="storeId"
          rules="required"
          :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.STORE')"
          :model-value="account.storeId"
        >
          <VcSelect
            v-model="account.storeId"
            :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.STORE')"
            :options="searchStores"
            option-value="id"
            option-label="name"
            searchable
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="userName"
          rules="required"
          :label="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.USER_NAME')"
          :model-value="account.userName"
        >
          <VcInput
            v-model="account.userName"
            :label="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.USER_NAME')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="email"
          rules="required|email"
          :label="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.EMAIL')"
          :model-value="account.email"
        >
          <VcInput
            v-model="account.email"
            type="email"
            :label="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.EMAIL')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="password"
          rules="required"
          :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.PASSWORD')"
          :model-value="account.password"
        >
          <VcInput
            v-model="account.password"
            type="password"
            :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.PASSWORD')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcSelect
          v-model="account.userType"
          :label="$t('CUSTOMER.PAGES.ACCOUNTS.COLUMNS.TYPE')"
          :options="toOptions(['Customer', 'Manager', 'Administrator'])"
          :clearable="false"
        />
        <VcSelect
          v-model="roleIds"
          :label="$t('CUSTOMER.FIELDS.ROLES')"
          :options="searchRoles"
          option-value="id"
          option-label="name"
          multiple
          searchable
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useBlade,
  useBladeForm,
  type ApplicationUser,
  type IBladeToolbar,
  type Role,
} from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { searchStores, securityApi, toOptions } from "../api";

defineBlade({
  url: "/customer-account-new",
  name: "CustomerAccountCreate",
  permissions: ["platform:security:create"],
});

// param: member id. Replaces the AngularJS "pick store" step + platform new-account wizard
const { param, callParent, closeSelf, setError } = useBlade();
const { t } = useI18n({ useScope: "global" });

const account = ref<ApplicationUser>({ userType: "Customer", memberId: param.value, roles: [] });
const roleIds = ref<string[]>([]);
const roles = new Map<string, Role>();
const loading = ref(false);

const form = useBladeForm({ data: account, autoBeforeClose: false });
onMounted(() => form.setBaseline());

async function searchRoles(keyword?: string, skip?: number) {
  const result = await (await securityApi()).searchRoles({ keyword, skip: skip ?? 0, take: 50 });
  (result.results ?? []).forEach((r) => roles.set(r.id!, r));
  return { results: result.results ?? [], totalCount: result.totalCount ?? 0 };
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "create",
    title: t("CUSTOMER.PAGES.DETAILS.TOOLBAR.CREATE"),
    icon: "lucide-save",
    disabled: computed(() => !form.canSave.value || loading.value),
    async clickHandler() {
      loading.value = true;
      try {
        const roleList = roleIds.value.map((id) => roles.get(id) ?? { id });
        const result = await (await securityApi()).create({ ...account.value, roles: roleList });
        if (!result.succeeded) {
          setError(result.errors?.join(", "));
          return;
        }
        await callParent("reload");
        await closeSelf();
      } finally {
        loading.value = false;
      }
    },
  },
]);
</script>
