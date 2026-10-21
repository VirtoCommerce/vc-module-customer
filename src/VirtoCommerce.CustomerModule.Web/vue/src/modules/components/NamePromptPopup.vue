<template>
  <VcPopup
    :title="title"
    @close="emit('cancel')"
  >
    <template #content>
      <div class="customer-prompt">
        <VcInput
          v-model="name"
          :label="label"
          required
          autofocus
          @keydown.enter="name.trim() && emit('confirm', name.trim())"
        />
      </div>
    </template>
    <template #footer>
      <VcButton
        variant="secondary"
        @click="emit('cancel')"
      >
        {{ $t("CUSTOMER.PAGES.ASSETS.CANCEL") }}
      </VcButton>
      <VcButton
        :disabled="!name.trim()"
        @click="emit('confirm', name.trim())"
      >
        {{ $t("CUSTOMER.PAGES.ASSETS.CREATE") }}
      </VcButton>
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
import { VcButton, VcInput, VcPopup } from "@vc-shell/framework/ui";
import { ref } from "vue";

// a one-field prompt in the framework popup (used for "New folder")
defineProps<{ title: string; label: string }>();
const emit = defineEmits<{ confirm: [name: string]; cancel: [] }>();
const name = ref("");
</script>

<style lang="scss">
.customer-prompt {
  padding: 4px 0 8px;
}
</style>
