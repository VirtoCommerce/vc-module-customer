<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.ICON.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="30%"
  >
    <div class="customer-add">
      <p class="customer-add__hint">{{ member.name }}</p>
      <VcImageUpload
        :image="member.iconUrl ? { url: member.iconUrl } : undefined"
        :loading="uploading"
        accept=".png,.jpg,.jpeg,.svg"
        @upload="upload"
        @remove="member.iconUrl = undefined"
      />
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcImageUpload } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { customerApi, uploadMemberIcon } from "../api";
import type { Member } from "../../api_client/virtocommerce.customer";

defineBlade({
  url: "/customer-member-icon",
  name: "CustomerMemberIcon",
});

// param: member id. Icon of the member linked to the signed-in user (the AngularJS icon widget on the user
// profile, which saved immediately)
const { param, closeSelf } = useBlade();
const { t } = useI18n({ useScope: "global" });

const member = ref<Member>({});
const loading = ref(false);
const uploading = ref(false);
const form = useBladeForm({ data: member, closeConfirmMessage: computed(() => t("CUSTOMER.ALERTS.CLOSE_CONFIRMATION")) });
let savedIconUrl: string | undefined; // the upload name alternates on the saved icon, not on the last upload

onMounted(async () => {
  loading.value = true;
  try {
    member.value = await (await customerApi()).getMemberById(param.value!);
    savedIconUrl = member.value.iconUrl;
    form.setBaseline();
  } finally {
    loading.value = false;
  }
});

async function upload(files: FileList) {
  if (!files[0]) return;
  uploading.value = true;
  try {
    member.value.iconUrl = await uploadMemberIcon(files[0], member.value.id, savedIconUrl);
  } finally {
    uploading.value = false;
  }
}

const toolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("CUSTOMER.PAGES.DETAILS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    disabled: computed(() => !form.isModified.value),
    async clickHandler() {
      loading.value = true;
      try {
        await (await customerApi()).updateMember(member.value);
        form.setBaseline();
        await closeSelf();
      } finally {
        loading.value = false;
      }
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
