<template>
  <VcCard :header="$t('CUSTOMER.CARDS.DYNAMIC_PROPERTIES')">
    <template
      v-if="multilingual && props.languages.length > 1"
      #actions
    >
      <VcSelect
        v-model="currentLanguage"
        class="customer-dp__lang"
        size="small"
        :options="props.languages.map((l) => ({ id: l, title: l }))"
        :clearable="false"
        :aria-label="$t('CUSTOMER.PAGES.SEO.FIELDS.LANGUAGE')"
      />
    </template>
    <div class="customer-dp">
      <VcDynamicProperty
        v-for="property in properties"
        :key="property.id + currentLanguage"
        :property="baseOf(property)"
        :model-value="valueOf(property)"
        :options-getter="loadDictionaries"
        :required="!!property.isRequired"
        :multivalue="!!property.isArray"
        :multilanguage="!!property.isMultilingual"
        :current-language="currentLanguage"
        :value-type="valueType(property.valueType)"
        :dictionary="!!property.isDictionary"
        :name="property.name ?? ''"
        :display-names="property.displayNames?.map((d) => ({ name: d.name, languageCode: d.locale }))"
        @update:model-value="(ev: unknown) => onChange(property, ev)"
      />
    </div>
  </VcCard>
</template>

<script setup lang="ts">
import { DynamicPropertiesClient, useApiClient, useDynamicProperties, type IBaseProperty } from "@vc-shell/framework";
import { VcCard, VcDynamicProperty, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { DynamicObjectProperty } from "../../api_client/virtocommerce.customer";

// Platform dynamic properties of a member (the platform dynamicPropertyWidget + propertyValue-list blade), edited
// with the framework's VcDynamicProperty. That control speaks the catalog property shape (languageCode, multivalue,
// dictionary, alias), so each platform property is mapped to it and the edited values mapped back.
// languages: the platform's (VirtoCommerce.Core.General.Languages), read by the member form
const props = defineProps<{ properties: DynamicObjectProperty[]; languages: string[] }>();
const { locale } = useI18n({ useScope: "global" });

const currentLanguage = ref<string>(locale.value);
const multilingual = computed(() => props.properties.some((p) => p.isMultilingual));
watch(
  () => props.languages,
  (languages) => (currentLanguage.value = languages[0] ?? locale.value),
  { immediate: true },
);

// platform value types the catalog control names differently
const valueType = (type?: string) =>
  ({ Decimal: "Number", Html: "LongText", Image: "ShortText" })[type ?? ""] ?? type ?? "ShortText";

function toBase(p: DynamicObjectProperty): IBaseProperty {
  return {
    id: p.id,
    name: p.name,
    valueType: valueType(p.valueType),
    multivalue: !!p.isArray,
    multilanguage: !!p.isMultilingual,
    dictionary: !!p.isDictionary,
    values: (p.values ?? []).map((v) => ({
      value: v.value,
      languageCode: v.locale ?? undefined,
      valueId: v.valueId ?? undefined,
      alias: p.isDictionary ? String(v.value ?? "") : undefined,
      propertyId: p.id,
    })),
  };
}

// one control property per property while its values are unchanged, so an edit re-renders only its own control
const bases = new WeakMap<DynamicObjectProperty, { values?: unknown; base: IBaseProperty }>();
function baseOf(p: DynamicObjectProperty) {
  let entry = bases.get(p);
  if (!entry || entry.values !== p.values) {
    entry = { values: p.values, base: toBase(p) };
    bases.set(p, entry);
  }
  return entry.base;
}

const { getPropertyValue, setPropertyValue, loadDictionaries } = useDynamicProperties({
  searchDictionary: async ({ propertyIds, keyword, skip, take }) => {
    const client = await useApiClient(DynamicPropertiesClient).getApiClient();
    const result = await client.searchDictionaryItems({ propertyId: propertyIds?.[0], keyword, skip, take: take ?? 100 });
    return (result.results ?? []).map((x) => ({
      id: x.id,
      propertyId: x.propertyId,
      alias: x.name,
      localizedValues: (x.displayNames ?? []).map((d) => ({ languageCode: d.locale, value: d.name })),
    }));
  },
});

// VcMultivalue (free-text multivalue) labels its chips by `title`, the values carry `value` (as in Catalog)
const isFreeMultivalue = (p: DynamicObjectProperty) => !!p.isArray && !p.isDictionary;
function valueOf(p: DynamicObjectProperty) {
  const value = getPropertyValue(baseOf(p), currentLanguage.value);
  return isFreeMultivalue(p) && Array.isArray(value)
    ? value.map((v) => ({ ...(v as object), title: (v as { value?: unknown }).value }))
    : value;
}

function onChange(p: DynamicObjectProperty, ev: unknown) {
  const change = (ev && typeof ev === "object" && "value" in ev ? { ...ev } : { value: ev }) as { value: unknown };
  if (isFreeMultivalue(p) && Array.isArray(change.value))
    change.value = change.value.map(({ title, ...v }: { title?: unknown; value?: unknown }) => ({ ...v, value: v.value ?? title }));
  const base = toBase(p);
  setPropertyValue({ property: base, ...change } as Parameters<typeof setPropertyValue>[0]);
  p.values = (base.values ?? []).map((v) => ({
    value: p.isDictionary ? (v.alias ?? v.value) : v.value,
    valueId: v.valueId || undefined,
    locale: p.isMultilingual ? v.languageCode : undefined,
    valueType: p.valueType,
    propertyId: p.id,
    propertyName: p.name,
  }));
}
</script>

<style lang="scss">
.customer-dp {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;

  &__lang {
    width: 120px;
  }
}
</style>
