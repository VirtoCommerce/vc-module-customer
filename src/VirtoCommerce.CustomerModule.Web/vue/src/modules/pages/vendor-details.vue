<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <MemberForm
      :item="item"
      member-type="Vendor"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { VcBlade } from "@vc-shell/framework/ui";
import MemberForm from "../components/MemberForm.vue";
import { useMemberBlade } from "../composables/useMemberBlade";

defineBlade({
  url: "/customer-vendor",
  name: "CustomerVendorDetails",
});

// exposes defineBladeContext({ item }) for other modules' widgets (targetBlades: ["CustomerVendorDetails"])
const { item, title, toolbar, loading } = useMemberBlade("Vendor");
</script>
