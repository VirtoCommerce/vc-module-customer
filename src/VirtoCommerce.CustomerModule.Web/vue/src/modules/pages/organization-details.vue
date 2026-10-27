<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <MemberForm
      :item="item"
      member-type="Organization"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { VcBlade } from "@vc-shell/framework/ui";
import MemberForm from "../components/MemberForm.vue";
import { useMemberBlade } from "../composables/useMemberBlade";

defineBlade({
  url: "/customer-organization",
  name: "CustomerOrganizationDetails",
});

// exposes defineBladeContext({ item }) for other modules' widgets (targetBlades: ["CustomerOrganizationDetails"])
const { item, title, toolbar, loading } = useMemberBlade("Organization");
</script>
