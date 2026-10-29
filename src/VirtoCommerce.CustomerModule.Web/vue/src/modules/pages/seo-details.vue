<template>
  <VcBlade
    :title="seo.semanticUrl || $t('CUSTOMER.PAGES.SEO.NEW')"
    :toolbar-items="toolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="customer-form tw-space-y-4">
        <VcSwitch
          :model-value="!!seo.isActive"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.ACTIVE')"
          @update:model-value="(v?: boolean) => (seo.isActive = !!v)"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcSelect
              v-model="seo.storeId"
              :label="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.STORE')"
              :options="searchStores"
              option-value="id"
              option-label="name"
              searchable
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="seo.languageCode"
              :label="$t('CUSTOMER.PAGES.SEO.FIELDS.LANGUAGE')"
              :options="toOptions([...languages, seo.languageCode])"
            />
          </VcCol>
        </VcRow>
        <VcInput
          v-model="seo.semanticUrl"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.URL_KEYWORD')"
          required
          :error="!!shownUrlError"
          :error-message="shownUrlError"
        />
        <VcInput
          v-model="seo.pageTitle"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.PAGE_TITLE')"
        />
        <VcTextarea
          v-model="seo.metaDescription"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.META_DESCRIPTION')"
        />
        <VcInput
          v-model="seo.metaKeywords"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.META_KEYWORDS')"
          maxlength="255"
        />
        <VcInput
          v-model="seo.imageAltDescription"
          :label="$t('CUSTOMER.PAGES.SEO.FIELDS.IMAGE_ALT')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getSettingValues, searchStores, toOptions } from "../api";
import type { SeoInfo } from "../../api_client/virtocommerce.customer";

// Not routable: the record arrives only through options
defineBlade({
  url: "/customer-seo-details",
  name: "CustomerSeoDetails",
  routable: false,
});

// options.seo: a copy of the record; options.index: its position (-1 = new). Apply hands it back to the list.
const { options, callParent, closeSelf } = useBlade<{ seo: SeoInfo; index: number }>();
const { t } = useI18n({ useScope: "global" });

const index = options.value?.index ?? -1;
const seo = ref<SeoInfo>({ ...(options.value?.seo ?? { isActive: true }) });
const languages = ref<string[]>([]);
const others = ref<SeoInfo[]>([]);

// legacy seo-utils: no URL-reserved characters; unique per store within the vendor
const INVALID = /[$+;=%{}[\]|\\/@ ~#!^*&?:'<>,]/;
const urlError = computed(() => {
  const value = seo.value.semanticUrl;
  if (!value) return t("CUSTOMER.PAGES.SEO.URL_REQUIRED");
  if (INVALID.test(value)) return t("CUSTOMER.PAGES.SEO.URL_INVALID");
  if (others.value.some((x) => x.storeId === seo.value.storeId && x.semanticUrl === value)) return t("CUSTOMER.PAGES.SEO.URL_DUPLICATE");
  return undefined;
});
// "required" shows only once the user typed something
const shownUrlError = computed(() => (seo.value.semanticUrl === undefined ? undefined : urlError.value));

const form = useBladeForm({
  data: seo,
  canSaveOverride: computed(() => !urlError.value),
  closeConfirmMessage: computed(() => t("CUSTOMER.ALERTS.CLOSE_CONFIRMATION")),
});

onMounted(async () => {
  languages.value = await getSettingValues("VirtoCommerce.Core.General.Languages");
  others.value = (await callParent<SeoInfo[]>("others", index)) ?? [];
  form.setBaseline();
});

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("CUSTOMER.PAGES.ADDRESS.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: computed(() => !form.canSave.value),
    async clickHandler() {
      await callParent("saveSeo", { seo: { ...seo.value }, index });
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
]);
</script>
