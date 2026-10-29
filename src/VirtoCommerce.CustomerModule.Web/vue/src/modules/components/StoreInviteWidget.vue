<template>
  <VcWidget
    icon="lucide-send"
    :title="$t('CUSTOMER.PAGES.LIST.TOOLBAR.INVITE')"
    :disabled="!store?.id"
    @click="invite"
  />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";

// "Invite customers" on the Store details blade (the AngularJS module added it to the store toolbar)
const ctx = injectBladeContext();
const store = computed(() => ctx.value.item as { id?: string } | undefined);
const { openBlade } = useBlade();

const invite = () => openBlade({ name: "CustomerInvite", options: { storeId: store.value?.id } });
</script>
