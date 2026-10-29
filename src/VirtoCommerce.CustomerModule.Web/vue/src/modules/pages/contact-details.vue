<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <MemberForm
      :item="item"
      member-type="Contact"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { VcBlade } from "@vc-shell/framework/ui";
import MemberForm from "../components/MemberForm.vue";
import { useMemberBlade } from "../composables/useMemberBlade";

defineBlade({
  url: "/customer-contact",
  name: "CustomerContactDetails",
});

// exposes defineBladeContext({ item }) for other modules' widgets (targetBlades: ["CustomerContactDetails"])
const { item, title, toolbar, loading } = useMemberBlade("Contact");
</script>
