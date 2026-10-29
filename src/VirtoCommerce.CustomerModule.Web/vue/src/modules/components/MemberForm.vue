<template>
  <VcContainer>
    <VcForm class="customer-form tw-space-y-4">
      <!-- names -->
      <VcRow
        v-if="isPerson"
        class="tw-gap-4"
      >
        <VcCol>
          <Field
            v-slot="{ errorMessage, handleChange, errors }"
            name="firstName"
            rules="required"
            :label="$t('CUSTOMER.FIELDS.FIRST_NAME')"
            :model-value="item.firstName"
          >
            <VcInput
              v-model="item.firstName"
              :label="$t('CUSTOMER.FIELDS.FIRST_NAME')"
              required
              :error="!!errors.length"
              :error-message="errorMessage"
              @update:model-value="handleChange"
              @blur="fillFullName"
            />
          </Field>
        </VcCol>
        <VcCol>
          <VcInput
            v-model="item.middleName"
            :label="$t('CUSTOMER.FIELDS.MIDDLE_NAME')"
          />
        </VcCol>
        <VcCol>
          <Field
            v-slot="{ errorMessage, handleChange, errors }"
            name="lastName"
            rules="required"
            :label="$t('CUSTOMER.FIELDS.LAST_NAME')"
            :model-value="item.lastName"
          >
            <VcInput
              v-model="item.lastName"
              :label="$t('CUSTOMER.FIELDS.LAST_NAME')"
              required
              :error="!!errors.length"
              :error-message="errorMessage"
              @update:model-value="handleChange"
              @blur="fillFullName"
            />
          </Field>
        </VcCol>
      </VcRow>
      <VcRow class="tw-gap-4">
        <VcCol v-if="isPerson">
          <VcInput
            v-model="item.fullName"
            :label="$t('CUSTOMER.FIELDS.FULL_NAME')"
            @blur="fillFullName(false)"
          />
        </VcCol>
        <VcCol v-else>
          <Field
            v-slot="{ errorMessage, handleChange, errors }"
            name="name"
            rules="required"
            :label="$t('CUSTOMER.FIELDS.NAME')"
            :model-value="item.name"
          >
            <VcInput
              v-model="item.name"
              :label="$t('CUSTOMER.FIELDS.NAME')"
              required
              :error="!!errors.length"
              :error-message="errorMessage"
              @update:model-value="handleChange"
            />
          </Field>
        </VcCol>
        <VcCol>
          <VcSelect
            v-model="item.status"
            :label="$t('CUSTOMER.FIELDS.STATUS')"
            :options="statusOptions"
          />
        </VcCol>
      </VcRow>
      <VcRow
        v-if="memberType === 'Contact' || memberType === 'Organization'"
        class="tw-gap-4"
      >
        <VcCol>
          <VcSelect
            v-model="item.groups"
            :label="$t('CUSTOMER.FIELDS.GROUPS')"
            :options="groupOptions"
            multiple
            searchable
          />
        </VcCol>
        <VcCol v-if="memberType === 'Contact'">
          <VcInput
            v-model="item.salutation"
            :label="$t('CUSTOMER.FIELDS.SALUTATION')"
          />
        </VcCol>
        <VcCol v-else>
          <VcInput
            v-model="item.businessCategory"
            :label="$t('CUSTOMER.FIELDS.BUSINESS_CATEGORY')"
          />
        </VcCol>
      </VcRow>
      <div
        v-if="memberType === 'Contact' || memberType === 'Organization'"
        class="customer-form__icon"
      >
        <VcLabel>{{ $t("CUSTOMER.FIELDS.ICON") }}</VcLabel>
        <VcImageUpload
          :image="item.iconUrl ? { url: item.iconUrl } : undefined"
          :loading="iconLoading"
          accept=".png,.jpg,.jpeg,.svg"
          @upload="uploadIcon"
          @remove="item.iconUrl = undefined"
        />
      </div>

      <!-- person details -->
      <VcCard
        v-if="isPerson"
        :header="$t('CUSTOMER.CARDS.DETAILS')"
      >
        <div class="customer-card tw-space-y-4">
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcSelect
                v-model="item.organizations"
                :label="$t('CUSTOMER.FIELDS.ORGANIZATIONS')"
                :options="searchOrganizations"
                option-value="id"
                option-label="name"
                multiple
                searchable
                @update:model-value="onOrganizationsChanged"
              />
            </VcCol>
            <VcCol>
              <VcSelect
                :key="(item.organizations ?? []).join()"
                v-model="item.defaultOrganizationId"
                :label="$t('CUSTOMER.FIELDS.DEFAULT_ORGANIZATION')"
                :options="selectedOrganizations"
                option-value="id"
                option-label="name"
              />
            </VcCol>
          </VcRow>
          <VcRow class="tw-gap-4">
            <VcCol v-if="memberType === 'Contact'">
              <VcSelect
                v-model="item.associatedOrganizations"
                :label="$t('CUSTOMER.FIELDS.ASSOCIATED_ORGANIZATIONS')"
                :options="searchOrganizations"
                option-value="id"
                option-label="name"
                multiple
                searchable
              />
            </VcCol>
            <VcCol>
              <VcInput
                v-model="birthDate"
                type="date"
                :label="$t('CUSTOMER.FIELDS.BIRTHDAY')"
              />
            </VcCol>
            <VcCol v-if="memberType === 'Employee'">
              <VcInput
                v-model="item.photoUrl"
                :label="$t('CUSTOMER.FIELDS.PHOTO_URL')"
              />
            </VcCol>
          </VcRow>
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcSelect
                v-model="item.timeZone"
                :label="$t('CUSTOMER.FIELDS.TIME_ZONE')"
                :options="timeZoneOptions"
                searchable
              />
            </VcCol>
            <VcCol>
              <VcSelect
                v-model="item.defaultLanguage"
                :label="$t('CUSTOMER.FIELDS.DEFAULT_LANGUAGE')"
                :options="languageOptions"
              />
            </VcCol>
          </VcRow>
          <template v-if="memberType === 'Contact'">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.currencyCode"
                  :label="$t('CUSTOMER.FIELDS.CURRENCY')"
                  :options="currencyOptions"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.taxPayerId"
                  :label="$t('CUSTOMER.FIELDS.TAX_PAYER_ID')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.preferredCommunication"
                  :label="$t('CUSTOMER.FIELDS.PREFERRED_COMMUNICATION')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.preferredDelivery"
                  :label="$t('CUSTOMER.FIELDS.PREFERRED_DELIVERY')"
                />
              </VcCol>
            </VcRow>
            <VcTextarea
              v-model="item.about"
              :label="$t('CUSTOMER.FIELDS.ABOUT')"
            />
          </template>
        </div>
      </VcCard>

      <!-- organization / vendor details -->
      <VcCard
        v-else
        :header="$t('CUSTOMER.CARDS.DETAILS')"
      >
        <div class="customer-card tw-space-y-4">
          <template v-if="memberType === 'Vendor'">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.siteUrl"
                  :label="$t('CUSTOMER.FIELDS.SITE_URL')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.logoUrl"
                  :label="$t('CUSTOMER.FIELDS.LOGO_URL')"
                />
              </VcCol>
            </VcRow>
            <VcInput
              v-model="item.groupName"
              :label="$t('CUSTOMER.FIELDS.GROUP_NAME')"
            />
          </template>
          <VcSelect
            v-else
            v-model="roleIds"
            :label="$t('CUSTOMER.FIELDS.ROLES')"
            :options="searchRoles"
            option-value="id"
            option-label="name"
            multiple
            searchable
          />
          <VcTextarea
            v-model="item.description"
            :label="$t('CUSTOMER.FIELDS.DESCRIPTION')"
          />
        </div>
      </VcCard>

      <VcCard :header="$t('CUSTOMER.CARDS.CONTACT_INFO')">
        <div class="customer-card">
          <VcRow class="tw-gap-4">
            <VcCol>
              <VcMultivalue
                v-model="emails"
                :label="$t('CUSTOMER.FIELDS.EMAILS')"
                :placeholder="$t('CUSTOMER.FIELDS.ADD_VALUE')"
                option-label="value"
                option-value="value"
              />
            </VcCol>
            <VcCol>
              <VcMultivalue
                v-model="phones"
                :label="$t('CUSTOMER.FIELDS.PHONES')"
                :placeholder="$t('CUSTOMER.FIELDS.ADD_VALUE')"
                option-label="value"
                option-value="value"
              />
            </VcCol>
          </VcRow>
        </div>
      </VcCard>

      <VcCard :header="$t('CUSTOMER.CARDS.ADDRESSES')">
        <template #actions>
          <VcButton
            size="sm"
            variant="secondary"
            icon="lucide-plus"
            @click="openAddress(-1)"
          >
            {{ $t("CUSTOMER.PAGES.ADDRESS.ADD") }}
          </VcButton>
        </template>
        <VcDataTable
          :items="addressRows"
          :state-key="'CUSTOMER_ADDRESSES'"
          :empty-state="{ icon: 'lucide-map-pin', title: $t('CUSTOMER.PAGES.ADDRESS.EMPTY') }"
          @row-click="(e: { data: { index: number } }) => openAddress(e.data.index)"
        >
          <VcColumn
            id="addressType"
            :title="$t('CUSTOMER.PAGES.ADDRESS.COLUMNS.TYPE')"
          />
          <VcColumn
            id="text"
            :title="$t('CUSTOMER.PAGES.ADDRESS.COLUMNS.ADDRESS')"
          />
          <VcColumn
            id="isDefault"
            :title="$t('CUSTOMER.PAGES.ADDRESS.COLUMNS.DEFAULT')"
            width="110px"
          >
            <template #body="{ data: row }">
              <VcIcon
                v-if="row.isDefault"
                icon="lucide-check"
                size="s"
              />
            </template>
          </VcColumn>
        </VcDataTable>
      </VcCard>

      <DynamicProperties
        v-if="item.dynamicProperties?.length"
        :properties="item.dynamicProperties"
        :languages="languages"
      />
    </VcForm>
  </VcContainer>
</template>

<script setup lang="ts">
import { useBlade } from "@vc-shell/framework";
import {
  VcButton,
  VcCard,
  VcCol,
  VcColumn,
  VcContainer,
  VcDataTable,
  VcForm,
  VcIcon,
  VcImageUpload,
  VcInput,
  VcLabel,
  VcMultivalue,
  VcRow,
  VcSelect,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref, watch } from "vue";
import {
  customerApi,
  getJson,
  getSettingValues,
  localDayToUtc,
  MEMBER_TYPES,
  organizationOptions,
  rolesOptions,
  timeZones,
  toOptions,
  utcDayToLocal,
  uploadMemberIcon,
  type MemberType,
} from "../api";
import type { MemberModel } from "../composables/useMemberBlade";
import type { Organization } from "../../api_client/virtocommerce.customer";
import DynamicProperties from "./DynamicProperties.vue";

const props = defineProps<{ item: MemberModel; memberType: MemberType }>();
const { openBlade } = useBlade();

const isPerson = computed(() => props.memberType === "Contact" || props.memberType === "Employee");

const statuses = ref<string[]>([]);
const groups = ref<string[]>([]);
const languages = ref<string[]>([]);
const currencies = ref<string[]>([]);

onMounted(async () => {
  [statuses.value, groups.value, languages.value, currencies.value] = await Promise.all([
    getSettingValues(MEMBER_TYPES[props.memberType].statusSetting),
    getSettingValues("Customer.MemberGroups"),
    getSettingValues("VirtoCommerce.Core.General.Languages"),
    props.memberType === "Contact"
      ? getJson<{ code: string }[]>("/api/currencies").then((x) => x.map((c) => c.code), () => [])
      : [],
  ]);
});

// built when their inputs change, not on every render of the form
const statusOptions = computed(() => toOptions([...statuses.value, props.item.status]));
const groupOptions = computed(() => toOptions([...groups.value, ...(props.item.groups ?? [])]));
const timeZoneOptions = computed(() => toOptions([...timeZones, props.item.timeZone]));
const languageOptions = computed(() => toOptions([...languages.value, props.item.defaultLanguage]));
const currencyOptions = computed(() => toOptions([...currencies.value, props.item.currencyCode]));

// legacy: leaving first/last name rebuilds the full name; an emptied full name is rebuilt too
function fillFullName(force: unknown = true) {
  const item = props.item;
  if (force !== false || !item.fullName)
    item.fullName = [item.firstName, item.lastName].filter(Boolean).join(" ") || item.fullName;
}

const birthDate = computed({
  get: () => utcDayToLocal(props.item.birthDate),
  set: (value: Date | string | null) => (props.item.birthDate = localDayToUtc(value)),
});

// emails/phones are plain string arrays; VcMultivalue works with objects
const asValues = (list?: string[]) => (list ?? []).map((value) => ({ id: value, value }));
const emails = computed({
  get: () => asValues(props.item.emails),
  set: (list: { value?: string }[]) => (props.item.emails = list.map((x) => x.value!)),
});
const phones = computed({
  get: () => asValues(props.item.phones),
  set: (list: { value?: string }[]) => (props.item.phones = list.map((x) => x.value!)),
});

// an organization is not its own parent
const searchOrganizations = organizationOptions(() => [props.item.id]);

// default organization is picked from the member's organizations
const selectedOrganizations = ref<Organization[]>([]);
async function loadSelectedOrganizations() {
  const ids = props.item.organizations ?? [];
  selectedOrganizations.value = ids.length ? await (await customerApi()).getOrganizationsByIds(ids) : [];
}
watch(() => props.item.organizations, loadSelectedOrganizations, { immediate: true });

function onOrganizationsChanged() {
  if (props.item.defaultOrganizationId && !props.item.organizations?.includes(props.item.defaultOrganizationId))
    props.item.defaultOrganizationId = undefined;
}

// organization roles: security roles, limited to the Customer.OrganizationRolesWhitelist setting when it is set
const { search: searchRoles, toRoles } = rolesOptions("Customer.OrganizationRolesWhitelist", () => props.item.roles ?? []);
const roleIds = computed({
  get: () => (props.item.roles ?? []).map((r) => r.roleId!),
  set: (ids: string[]) => (props.item.roles = toRoles(ids, props.item.roles)),
});

// member icon (the AngularJS member-icon widget/blade)
const iconLoading = ref(false);
// the item is replaced on every load and revert, never by an edit: its icon then is the saved one
let savedIconUrl: string | undefined;
watch(() => props.item, (item) => (savedIconUrl = item.iconUrl), { immediate: true });
async function uploadIcon(files: FileList) {
  if (!files[0]) return;
  iconLoading.value = true;
  try {
    props.item.iconUrl = await uploadMemberIcon(files[0], props.item.id, savedIconUrl);
  } finally {
    iconLoading.value = false;
  }
}

const addressRows = computed(() =>
  (props.item.addresses ?? []).map((a, index) => ({
    ...a,
    index,
    id: String(index),
    text: [a.line1, a.line2, a.city, a.regionName, a.postalCode, a.countryName ?? a.countryCode]
      .filter(Boolean)
      .join(", "),
  })),
);

function openAddress(index: number) {
  openBlade({
    name: "CustomerAddressDetails",
    options: { address: index >= 0 ? { ...props.item.addresses![index] } : undefined, index },
  });
}
</script>

<style lang="scss">
/* the design's blade body padding, as in the Orders details */
.customer-form {
  padding: 20px 22px 36px;

  /* let long select values shrink inside their column instead of overflowing the next one */
  .vc-col,
  .vc-select__field-wrapper {
    min-width: 0;
  }
}

.customer-card {
  padding: 16px;
}

.customer-form__icon {
  max-width: 220px;
}
</style>
