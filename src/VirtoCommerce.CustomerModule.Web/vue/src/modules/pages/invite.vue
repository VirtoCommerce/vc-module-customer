<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.INVITE.TITLE')"
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
          :model-value="request.storeId"
        >
          <VcSelect
            v-model="request.storeId"
            :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.STORE')"
            :options="searchStoresWithLanguages"
            option-value="id"
            option-label="name"
            searchable
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="
              (v: unknown) => {
                handleChange(v);
                onStoreChanged();
              }
            "
          />
        </Field>
        <VcSelect
          v-model="request.cultureName"
          :label="$t('CUSTOMER.FIELDS.DEFAULT_LANGUAGE')"
          :options="toOptions(storeLanguages)"
        />
        <VcSelect
          v-model="request.organizationId"
          :label="$t('CUSTOMER.PAGES.MEMBERSHIPS.COLUMNS.ORGANIZATION')"
          :options="searchOrganizations"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcSelect
          v-model="roleId"
          :label="$t('CUSTOMER.PAGES.INVITE.ROLE')"
          :options="roles"
          option-value="id"
          option-label="name"
        />
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="emails"
          rules="required"
          :label="$t('CUSTOMER.FIELDS.EMAILS')"
          :model-value="request.emails"
        >
          <VcMultivalue
            v-model="emails"
            :label="$t('CUSTOMER.FIELDS.EMAILS')"
            :placeholder="$t('CUSTOMER.PAGES.INVITE.EMAILS_PLACEHOLDER')"
            option-label="value"
            option-value="value"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange(request.emails)"
          />
        </Field>
        <VcTextarea
          v-model="request.message"
          :label="$t('CUSTOMER.PAGES.INVITE.MESSAGE')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcMultivalue, VcSelect, VcTextarea } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { customerApi, organizationOptions, searchStores, toOptions, type Store } from "../api";
import type { InviteCustomerRequest } from "../../api_client/virtocommerce.customer";

defineBlade({
  url: "/customer-invite",
  name: "CustomerInvite",
  permissions: ["customer:invite"],
});

// opened from the member list (options.organizationId) or a store's details (options.storeId)
const { options, setError, clearError } = useBlade<{ organizationId?: string; storeId?: string }>();
const { t } = useI18n({ useScope: "global" });
const { showInfo } = usePopup();

const request = ref<InviteCustomerRequest>({
  storeId: options.value?.storeId,
  organizationId: options.value?.organizationId,
  emails: [],
});
const roleId = ref<string>();
const roles = ref<{ id: string; name: string }[]>([]);
const stores = new Map<string, Store>();
const storeLanguages = ref<string[]>([]);
const loading = ref(false);

const form = useBladeForm({ data: request, autoBeforeClose: false });

const emails = computed({
  get: () => (request.value.emails ?? []).map((value) => ({ id: value, value })),
  // pasted lists ("a@x.com; b@y.com") become separate emails, as in the AngularJS blade
  set: (list: { value?: string }[]) =>
    (request.value.emails = [
      ...new Set(list.flatMap((x) => (x.value ?? "").split(/[\s,;]+/)).filter((x) => x.includes("@"))),
    ]),
});

async function searchStoresWithLanguages(keyword?: string, skip?: number, ids?: string[]) {
  const result = await searchStores(keyword, skip, ids);
  result.results.forEach((s) => stores.set(s.id, s));
  if (ids?.length) onStoreChanged(false);
  return result;
}

function onStoreChanged(resetLanguage = true) {
  const store = stores.get(request.value.storeId!);
  storeLanguages.value = store?.languages ?? [];
  if (resetLanguage || !request.value.cultureName) request.value.cultureName = store?.defaultLanguage;
}

const searchOrganizations = organizationOptions();

onMounted(async () => {
  form.setBaseline();
  const result = await (await customerApi()).getInviteRoles();
  roles.value = (result.results ?? []).map((r) => ({ id: r.id!, name: r.name ?? r.id! }));
  // the first allowed role is preselected, as in the AngularJS blade
  roleId.value ??= roles.value[0]?.id;
});

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "send",
    title: t("CUSTOMER.PAGES.INVITE.SEND"),
    icon: "lucide-send",
    disabled: computed(() => !request.value.storeId || !request.value.emails?.length || loading.value),
    async clickHandler() {
      loading.value = true;
      clearError();
      try {
        const result = await (
          await customerApi()
        ).inviteCustomers({ ...request.value, roleIds: roleId.value ? [roleId.value] : undefined });
        if (result.errors?.length) {
          setError(result.errors.map((e) => e.description).join("; "));
          return;
        }
        request.value.emails = [];
        showInfo(t("CUSTOMER.PAGES.INVITE.SENT"));
      } finally {
        loading.value = false;
      }
    },
  },
]);
</script>
