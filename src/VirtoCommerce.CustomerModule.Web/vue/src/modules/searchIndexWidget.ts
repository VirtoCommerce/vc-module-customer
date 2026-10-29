import type { App, Ref } from "vue";

/** The part of the Admin Shell's shared widgets this module uses (vc-module-admin-shell, docs/blade-widgets.md) */
interface SharedWidgets {
  place(placement: {
    id: string;
    widget: string;
    targetBlades: string[];
    title?: string;
    props?: Record<string, unknown>;
    isVisible?: boolean | Ref<boolean> | ((blade?: { param?: string }) => boolean);
  }): void;
}

/**
 * Search's "Index" widget on the contact and organization blades: the entity's index document, and a link to it. Search provides it; this
 * module places it (AngularJS: customer.js registered virtoCommerce.searchModule.indexWidgetController here). Without
 * Search installed, or in a host without shared widgets, nothing shows.
 */
export function placeSearchIndexWidget(app: App) {
  const shared = app.config.globalProperties.$sharedWidgets as SharedWidgets | undefined;
  const targets: [blade: string, documentType: string][] = [
    ["CustomerContactDetails", "Member"],
    ["CustomerOrganizationDetails", "Member"],
  ];
  for (const [blade, documentType] of targets) {
    shared?.place({
      id: `${blade}SearchIndex`,
      widget: "search.index",
      targetBlades: [blade],
      title: "Index",
      props: { documentType },
      // saved entities only: a new one has no index document (AngularJS showed none there either)
      isVisible: (b) => !!b?.param,
    });
  }
}
