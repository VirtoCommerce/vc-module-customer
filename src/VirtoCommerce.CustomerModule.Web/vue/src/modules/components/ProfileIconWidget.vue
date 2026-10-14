<template>
  <VcWidget
    icon="lucide-image"
    :title="$t('CUSTOMER.WIDGETS.ICON')"
    :disabled="!memberId || !canEdit"
    @click="open"
  />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";

// Icon of the member linked to the signed-in user, on the user profile blade (legacy iconWidget on userProfileDetail)
const ctx = injectBladeContext();
const memberId = computed(() => (ctx.value.item as { memberId?: string } | undefined)?.memberId);
const { hasAccess } = usePermissions();
const canEdit = computed(() => hasAccess("customer:update") || hasAccess("customer:create"));
const { openBlade } = useBlade();

const open = () => memberId.value && openBlade({ name: "CustomerMemberIcon", param: memberId.value });
</script>
