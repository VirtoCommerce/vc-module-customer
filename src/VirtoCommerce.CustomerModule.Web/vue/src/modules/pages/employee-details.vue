<template>
  <VcBlade
    :title="title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <MemberForm
      :item="item"
      member-type="Employee"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { VcBlade } from "@vc-shell/framework/ui";
import MemberForm from "../components/MemberForm.vue";
import { useMemberBlade } from "../composables/useMemberBlade";

defineBlade({
  url: "/customer-employee",
  name: "CustomerEmployeeDetails",
});

// exposes defineBladeContext({ item }) for other modules' widgets (targetBlades: ["CustomerEmployeeDetails"])
const { item, title, toolbar, loading } = useMemberBlade("Employee");
</script>
