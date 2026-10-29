<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="customer-form tw-space-y-4">
        <VcTextarea
          v-model="address.description"
          :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.DESCRIPTION')"
          maxlength="128"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="address.addressType"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.TYPE')"
              :options="toOptions([...ADDRESS_TYPES, address.addressType])"
              :clearable="false"
            />
          </VcCol>
          <VcCol>
            <VcSwitch
              :model-value="!!address.isDefault"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.DEFAULT')"
              :disabled="address.addressType === 'BillingAndShipping'"
              @update:model-value="(v?: boolean) => (address.isDefault = !!v)"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="address.firstName"
              :label="$t('CUSTOMER.FIELDS.FIRST_NAME')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.middleName"
              :label="$t('CUSTOMER.FIELDS.MIDDLE_NAME')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.lastName"
              :label="$t('CUSTOMER.FIELDS.LAST_NAME')"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="countryCode"
              rules="required"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.COUNTRY')"
              :model-value="address.countryCode"
            >
              <VcSelect
                v-model="address.countryCode"
                :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.COUNTRY')"
                :options="countries"
                option-value="id"
                option-label="name"
                searchable
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="
                  (v: unknown) => {
                    handleChange(v);
                    onCountryChanged();
                  }
                "
              />
            </Field>
          </VcCol>
          <VcCol>
            <VcSelect
              v-if="regions.length"
              v-model="address.regionId"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.REGION')"
              :options="regions"
              option-value="id"
              option-label="name"
              searchable
              @update:model-value="onRegionChanged"
            />
            <VcInput
              v-else
              v-model="address.regionName"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.REGION')"
              @update:model-value="address.regionId = undefined"
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="city"
              rules="required"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.CITY')"
              :model-value="address.city"
            >
              <VcInput
                v-model="address.city"
                :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.CITY')"
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
          <VcCol>
            <Field
              v-slot="{ errorMessage, handleChange, errors }"
              name="postalCode"
              rules="required"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.ZIP')"
              :model-value="address.postalCode"
            >
              <VcInput
                v-model="address.postalCode"
                :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.ZIP')"
                required
                :error="!!errors.length"
                :error-message="errorMessage"
                @update:model-value="handleChange"
              />
            </Field>
          </VcCol>
        </VcRow>
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="line1"
          rules="required"
          :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.LINE1')"
          :model-value="address.line1"
        >
          <VcInput
            v-model="address.line1"
            :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.LINE1')"
            required
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcInput
          v-model="address.line2"
          :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.LINE2')"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="address.email"
              type="email"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.EMAIL')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="address.phone"
              :label="$t('CUSTOMER.PAGES.ADDRESS.FIELDS.PHONE')"
            />
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCol,
  VcContainer,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getCountries, getRegions, toOptions, type Country } from "../api";
import { AddressType, type CustomerAddress } from "../../api_client/virtocommerce.customer";

// Not routable: the address arrives only through options
defineBlade({
  url: "/customer-address",
  name: "CustomerAddressDetails",
  routable: false,
});

const ADDRESS_TYPES = [AddressType.Billing, AddressType.Shipping, AddressType.BillingAndShipping];

// the address is a copy; Save hands it back to the member blade (saved together with the member)
const { options, callParent, closeSelf } = useBlade<{ address?: CustomerAddress; index: number }>();
const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();

const index = computed(() => options.value?.index ?? -1);
const isNew = computed(() => index.value < 0);
const address = ref<CustomerAddress>({ ...(options.value?.address ?? { addressType: AddressType.Shipping }) });

const form = useBladeForm({
  data: address,
  closeConfirmMessage: computed(() => t("CUSTOMER.ALERTS.CLOSE_CONFIRMATION")),
});

const title = computed(() =>
  isNew.value
    ? t("CUSTOMER.PAGES.ADDRESS.NEW")
    : [address.value.line1, address.value.city].filter(Boolean).join(", ") || t("CUSTOMER.PAGES.ADDRESS.TITLE"),
);

const countries = ref<Country[]>([]);
const regions = ref<Country[]>([]);

async function loadRegions() {
  regions.value = address.value.countryCode ? await getRegions(address.value.countryCode).catch(() => []) : [];
}

onMounted(async () => {
  countries.value = await getCountries();
  await loadRegions();
  // existing addresses may carry a region name only; match it to the country's region list
  if (!address.value.regionId && address.value.regionName) {
    address.value.regionId = regions.value.find((r) => r.name === address.value.regionName)?.id;
  }
  form.setBaseline();
});

async function onCountryChanged() {
  address.value.countryName = countries.value.find((c) => c.id === address.value.countryCode)?.name;
  address.value.regionId = undefined;
  address.value.regionName = undefined;
  await loadRegions();
}

function onRegionChanged() {
  address.value.regionName = regions.value.find((r) => r.id === address.value.regionId)?.name;
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("CUSTOMER.PAGES.ADDRESS.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: computed(() => !form.canSave.value),
    async clickHandler() {
      await callParent("saveAddress", { address: { ...address.value }, index: index.value });
      form.setBaseline();
      await closeSelf();
    },
  },
  {
    id: "reset",
    title: t("CUSTOMER.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    isVisible: computed(() => !isNew.value),
    async clickHandler() {
      if (!(await showConfirmation(t("CUSTOMER.ALERTS.DELETE_ADDRESS")))) return;
      await callParent("deleteAddress", index.value);
      form.setBaseline();
      await closeSelf();
    },
  },
]);
</script>
