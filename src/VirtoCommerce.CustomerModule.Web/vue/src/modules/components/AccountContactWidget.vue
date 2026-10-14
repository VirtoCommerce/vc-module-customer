<template>
  <VcWidget
    icon="lucide-contact"
    :title="member ? $t('CUSTOMER.WIDGETS.SHOW_CONTACT') : $t('CUSTOMER.WIDGETS.NO_CONTACT')"
    :disabled="!member"
    @click="open"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { customerApi, MEMBER_TYPES, type MemberType } from "../api";
import type { Member } from "../../api_client/virtocommerce.customer";

// The contact linked to a security account, on the platform's user details blade (legacy accountContactWidget)
const ctx = injectBladeContext();
const memberId = computed(() => (ctx.value.item as { memberId?: string } | undefined)?.memberId);
const member = ref<Member>();
const { openBlade } = useBlade();

watch(
  memberId,
  async (id) => {
    member.value = id ? await (await customerApi()).getMemberById(id).catch(() => undefined) : undefined;
  },
  { immediate: true },
);

function open() {
  const m = member.value;
  if (m) openBlade({ name: MEMBER_TYPES[m.memberType as MemberType]?.blade ?? "CustomerContactDetails", param: m.id });
}
</script>
