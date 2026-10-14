<template>
  <VcBlade
    :title="$t('CUSTOMER.WIDGETS.ASSETS')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="customer-list__crumbs">
      <VcBreadcrumbs :items="breadcrumbs" />
    </div>
    <VcDataTable
      v-model:search-value="keyword"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      data-key="url"
      :searchable="true"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      :state-key="'CUSTOMER_ORG_ASSETS'"
      :empty-state="{ icon: 'lucide-folder-open', title: $t('CUSTOMER.PAGES.ASSETS.EMPTY') }"
      @row-click="(e: { data: AssetEntry }) => onRowClick(e.data)"
    >
      <VcColumn
        id="name"
        :title="$t('CUSTOMER.PAGES.LIST.COLUMNS.NAME')"
        always-visible
      >
        <template #body="{ data: entry }">
          <div class="customer-list__name">
            <VcImage
              v-if="entry.contentType?.startsWith('image/')"
              :src="entry.url"
              size="xs"
              bordered
              class="customer-assets__thumb"
            />
            <VcIcon
              v-else
              :icon="entry.type === 'folder' ? 'lucide-folder' : 'lucide-file'"
              size="m"
              class="customer-list__icon"
            />
            <span>{{ entry.name }}</span>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="size"
        :title="$t('CUSTOMER.PAGES.ASSETS.SIZE')"
        align="end"
        width="90px"
      >
        <template #body="{ data: entry }">{{ entry.type === "folder" ? "" : readableSize(entry.size, 1) }}</template>
      </VcColumn>
      <VcColumn
        id="modifiedDate"
        :title="$t('CUSTOMER.PAGES.ASSETS.MODIFIED')"
        type="date-ago"
        width="150px"
      />
    </VcDataTable>
    <input
      :ref="setFileInput"
      type="file"
      multiple
      hidden
      @change="onFiles"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { readableSize, useBlade, useFunctions, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcBreadcrumbs, VcColumn, VcDataTable, VcIcon, VcImage } from "@vc-shell/framework/ui";
import NamePromptPopup from "../components/NamePromptPopup.vue";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { assets, type AssetEntry } from "../api";

defineBlade({
  url: "/customer-org-assets",
  name: "CustomerOrganizationAssets",
  permissions: ["platform:asset:read"],
});

// param: organization id. Its asset folder "organizations/<id>" (the AngularJS organization assets widget)
const { param, options } = useBlade<{ name?: string }>();
const { t } = useI18n({ useScope: "global" });
const { debounce } = useFunctions();
const { hasAccess } = usePermissions();
const { showConfirmation } = usePopup();

// "New folder" asks for the name in a framework popup (legacy: dialogService prompt)
const { open: openFolderPrompt, close: closeFolderPrompt } = usePopup({
  component: NamePromptPopup,
  props: { title: t("CUSTOMER.PAGES.ASSETS.NEW_FOLDER"), label: t("CUSTOMER.PAGES.ASSETS.FOLDER_NAME") },
  emits: {
    async onConfirm(name: string) {
      closeFolderPrompt();
      await assets.createFolder(folder.value, name);
      await load();
    },
    onCancel: () => closeFolderPrompt(),
  },
});

const root = computed(() => `organizations/${param.value}`);
const path = ref<string[]>([]); // sub folders below the organization folder
const folder = computed(() => [root.value, ...path.value].join("/"));
const items = ref<AssetEntry[]>([]);
const selection = ref<AssetEntry[]>([]);
const keyword = ref("");
const loading = ref(false);
let fileInput: HTMLInputElement | undefined;
// function ref: object refs do not bind inside a remote (porting guide gotcha 1)
const setFileInput = (el: unknown) => (fileInput = (el as HTMLInputElement) ?? undefined);

async function load() {
  loading.value = true;
  try {
    items.value = await assets.list(folder.value, keyword.value || undefined).catch(() => []);
    selection.value = [];
  } finally {
    loading.value = false;
  }
}
load();
watch(keyword, debounce(load, 300));

const breadcrumbs = computed(() => [
  { id: "", title: options.value?.name ?? t("CUSTOMER.WIDGETS.ASSETS"), icon: "lucide-folder", clickHandler: () => goTo(0) },
  ...path.value.map((name, i) => ({ id: String(i + 1), title: name, clickHandler: () => goTo(i + 1) })),
]);

function goTo(depth: number) {
  path.value = path.value.slice(0, depth);
  load();
}

function onRowClick(entry: AssetEntry) {
  if (entry.type === "folder") {
    path.value = [...path.value, entry.name];
    load();
  } else window.open(entry.url, "_blank");
}

async function onFiles() {
  const files = [...(fileInput?.files ?? [])];
  if (!files.length) return;
  // existing names are replaced only after a confirmation, as in the AngularJS upload blade
  const existing = files.filter((f) => items.value.some((x) => x.name === f.name)).map((f) => f.name);
  const overwrite = existing.length > 0 && (await showConfirmation(t("CUSTOMER.PAGES.ASSETS.OVERWRITE", { names: existing.join(", ") })));
  const toUpload = overwrite ? files : files.filter((f) => !existing.includes(f.name));
  loading.value = true;
  try {
    if (toUpload.length) await assets.upload(folder.value, toUpload, overwrite);
  } finally {
    fileInput!.value = "";
    await load();
  }
}

async function remove(entries: AssetEntry[]) {
  if (!(await showConfirmation(t("CUSTOMER.PAGES.ASSETS.DELETE", { count: entries.length })))) return;
  loading.value = true;
  try {
    await assets.remove(entries.map((x) => x.url));
  } finally {
    await load();
  }
}

const rowActions = (entry: AssetEntry) => [
  {
    id: "download",
    title: t("CUSTOMER.PAGES.ASSETS.DOWNLOAD"),
    icon: "lucide-download",
    type: "info" as const,
    hidden: entry.type === "folder",
    clickHandler: () => window.open(entry.url, "_blank"),
  },
  {
    id: "copy",
    title: t("CUSTOMER.PAGES.ASSETS.COPY_URL"),
    icon: "lucide-copy",
    type: "info" as const,
    hidden: entry.type === "folder",
    clickHandler: () => navigator.clipboard.writeText(entry.url),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    type: "danger" as const,
    variant: "danger" as const,
    hidden: !hasAccess("platform:asset:delete"),
    clickHandler: () => remove([entry]),
  },
];

const toolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("CUSTOMER.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "folder",
    title: t("CUSTOMER.PAGES.ASSETS.NEW_FOLDER"),
    icon: "lucide-folder-plus",
    permissions: "platform:asset:create",
    async clickHandler() {
      openFolderPrompt();
    },
  },
  {
    id: "upload",
    title: t("CUSTOMER.PAGES.ASSETS.UPLOAD"),
    icon: "lucide-upload",
    permissions: "platform:asset:create",
    clickHandler: () => fileInput?.click(),
  },
  {
    id: "delete",
    title: t("CUSTOMER.PAGES.LIST.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "platform:asset:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => remove(selection.value),
  },
]);
</script>

<style lang="scss">
.customer-assets__thumb {
  flex-shrink: 0;
}
</style>
