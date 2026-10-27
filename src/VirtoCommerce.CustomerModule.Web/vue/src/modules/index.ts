import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw, type App } from "vue";
import AccountContactWidget from "./components/AccountContactWidget.vue";
import ProfileIconWidget from "./components/ProfileIconWidget.vue";
import StoreInviteWidget from "./components/StoreInviteWidget.vue";
import { registerCustomerSearch } from "./search";
import { placeSearchIndexWidget } from "./searchIndexWidget";

// Widgets the AngularJS Customer module added to other modules' blades, registered by blade name.
// Each shows only on a saved entity (blade param), never on a "new" blade.

// invite customers from a store (legacy: toolbar command on storeDetailController)
registerExternalWidget({
  id: "CustomerStoreInvite",
  title: "Invite customers",
  component: markRaw(StoreInviteWidget),
  targetBlades: ["StoreDetails"],
  isVisible: (blade) => !!blade?.param,
});

// the account's contact (legacy accountContactWidget on 'accountDetail'); not when the account was opened
// from that contact's own Accounts list
registerExternalWidget({
  id: "CustomerAccountContact",
  title: "Contact",
  component: markRaw(AccountContactWidget),
  targetBlades: ["PlatformSecurityUserDetails"],
  isVisible: (blade) => !!blade?.param && !blade?.options?.fromContact,
});

// the signed-in user's member icon (legacy iconWidget on 'userProfileDetail')
registerExternalWidget({
  id: "CustomerProfileIcon",
  title: "Icon",
  component: markRaw(ProfileIconWidget),
  targetBlades: ["PlatformUserProfile"],
});

const appModule = defineAppModule({
  blades,
  locales,
});

export default {
  install(app: App) {
    appModule.install(app);
    // customers in the Admin Shell's global search
    registerCustomerSearch(app);
    // Search's Index widget on the contact and organization blades
    placeSearchIndexWidget(app);
  },
};

export * from "./pages";
