<template>
  <VcBlade
    :title="$t('CUSTOMER.WIDGETS.SEO')"
    :toolbar-items="toolbar"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :active-item-id="activeId"
      :state-key="'CUSTOMER_SEO'"
      :empty-state="{ icon: 'lucide-globe', title: $t('CUSTOMER.PAGES.SEO.EMPTY') }"
      @row-click="(e: { data: Row }) => open(e.data.index)"
    >
      <VcColumn
        id="semanticUrl"
        :title="$t('CUSTOMER.PAGES.SEO.FIELDS.URL_KEYWORD')"
        always-visible
      />
      <VcColumn
        id="storeId"
        :title="$t('CUSTOMER.PAGES.ACCOUNT_CREATE.STORE')"
      />
      <VcColumn
        id="languageCode"
        :title="$t('CUSTOMER.PAGES.SEO.FIELDS.LANGUAGE')"
      />
      <VcColumn
        id="isActive"
        :title="$t('CUSTOMER.PAGES.SEO.FIELDS.ACTIVE')"
        width="90px"
      >
        <template #body="{ data: row }">
          <VcIcon
            v-if="row.isActive"
            icon="lucide-check"
            size="s"
          />
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcIcon } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { SeoInfo } from "../../api_client/virtocommerce.customer";

// Not routable: the records are the vendor blade's unsaved state
defineBlade({
  url: "/customer-seo",
  name: "CustomerSeoList",
  routable: false,
});

// The vendor's SEO records (the AngularJS seo-list blade the vendor SEO widget opened); edited in place on
// the vendor blade and saved with the vendor.
type Row = SeoInfo & { index: number; id: string };

const { openBlade, callParent, closeChildren, exposeToChildren } = useBlade();
const { t } = useI18n({ useScope: "global" });
const { showConfirmation } = usePopup();

const seoInfos = ref<SeoInfo[]>([]);
const selection = ref<Row[]>([]);
const activeId = ref<string>();
const rows = computed<Row[]>(() => seoInfos.value.map((s, index) => ({ ...s, index, id: String(index) })));

const load = async () => (seoInfos.value = (await callParent<SeoInfo[]>("seoInfos")) ?? []);
onMounted(load);

function open(index: number) {
  activeId.value = index >= 0 ? String(index) : undefined;
  openBlade({
    name: "CustomerSeoDetails",
    options: { seo: index >= 0 ? { ...seoInfos.value[index] } : { isActive: true }, index },
    onClose: () => (activeId.value = undefined),
  });
}

// the details blade saves through here to the vendor, then the list re-reads it
exposeToChildren({
  saveSeo: async (args: unknown) => {
    await callParent("saveSeo", args);
    await load();
  },
  others: (index: unknown) => seoInfos.value.filter((_, i) => i !== index),
});

const toolbar = ref<IBladeToolbar[]>([
  { id: "add", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.ADD"), icon: "lucide-plus", permissions: "customer:update", clickHandler: () => open(-1) },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "customer:update",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("CUSTOMER.ALERTS.DELETE_SEO", { count: selection.value.length })))) return;
      await closeChildren();
      await callParent("deleteSeo", selection.value.map((x) => x.index));
      selection.value = [];
      await load();
    },
  },
]);
</script>
