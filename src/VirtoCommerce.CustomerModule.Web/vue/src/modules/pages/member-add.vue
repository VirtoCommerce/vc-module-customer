<template>
  <VcBlade
    :title="$t('CUSTOMER.PAGES.ADD.TITLE')"
    width="30%"
  >
    <div class="customer-add">
      <p class="customer-add__hint">{{ $t("CUSTOMER.PAGES.ADD.SUBTITLE") }}</p>
      <button
        v-for="type in types"
        :key="type"
        type="button"
        class="customer-add__item"
        @click="add(type)"
      >
        <VcIcon
          :icon="MEMBER_TYPES[type].icon"
          size="l"
          class="customer-add__icon"
        />
        <span>
          <span class="customer-add__title">{{ $t(`CUSTOMER.TYPES.${type.toUpperCase()}`) }}</span>
          <span class="customer-add__descr">{{ $t(`CUSTOMER.PAGES.ADD.${type.toUpperCase()}`) }}</span>
        </span>
      </button>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcIcon } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { MEMBER_TYPES, type MemberType } from "../api";

// Not routable: the organization arrives only through options
defineBlade({
  url: "/customer-add",
  name: "CustomerMemberAdd",
  permissions: ["customer:create"],
  routable: false,
});

// options: the organization the user is browsing (legacy member-add: only its knownChildrenTypes inside an organization)
const { options, replaceWith } = useBlade<{ parentId?: string; parentType?: MemberType }>();

const types = computed(
  () =>
    (options.value?.parentType && MEMBER_TYPES[options.value.parentType].children) ||
    (Object.keys(MEMBER_TYPES) as MemberType[]),
);

function add(type: MemberType) {
  replaceWith({ name: MEMBER_TYPES[type].blade, options: { parentId: options.value?.parentId } });
}
</script>

<style lang="scss">
.customer-add {
  padding: 20px 22px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.customer-add__hint {
  margin: 0 0 8px;
  color: var(--color-neutral-500);
  font-size: 13px;
}

.customer-add__item {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 14px 16px;
  border: 1px solid var(--color-neutral-200);
  border-radius: 8px;
  background: none;
  text-align: left;
  cursor: pointer;
  font: inherit;

  &:hover {
    border-color: var(--color-primary-500);
    background: var(--color-secondary-50);
  }
}

.customer-add__icon {
  color: var(--color-primary-500);
  flex-shrink: 0;
}

.customer-add__title {
  display: block;
  font-weight: 600;
  color: var(--color-neutral-900);
}

.customer-add__descr {
  display: block;
  margin-top: 2px;
  font-size: 13px;
  color: var(--color-neutral-500);
}
</style>
