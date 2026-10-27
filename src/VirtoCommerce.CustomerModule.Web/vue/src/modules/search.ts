// Customers in the Admin Shell's global search (the header box, Ctrl+K): this module's provider, registered with the
// shell's registry (app.config.globalProperties.$globalSearch; see the shell's docs/global-search.md). A result opens the
// member's details over the contacts list, as the list opens it; "Show all" opens the list filtered by the query.
import { ref, type App } from "vue";
import type { Contact } from "../api_client/virtocommerce.customer";
import { MEMBER_TYPES, type MemberType } from "./api";

/** The part of the shell's global search registry this module uses */
interface GlobalSearch {
  registerProvider(provider: object): void;
  hasPermission(permission: string | string[]): boolean;
  rankItems<T>(items: T[], query: string, fields: (item: T) => unknown[], take?: number): T[];
  openBlade(options: {
    route: string;
    workspace: string;
    blades?: { name: string; param?: string; options?: Record<string, unknown> }[];
  }): Promise<void>;
}

/** The keyword "Show all" asks the contacts list to show; the list takes it (pages/members-list.vue) */
export const requestedKeyword = ref<string>();

// a member as the search returns it: any type, with the fields shown here
type FoundMember = Pick<Contact, "id" | "name" | "fullName" | "memberType" | "emails">;

/** Registers the Customers search provider; nothing in a host without global search */
export function registerCustomerSearch(app: App) {
  const search = app.config.globalProperties.$globalSearch as GlobalSearch | undefined;
  if (!search) return;

  const openList = () => search.openBlade({ route: "/customers", workspace: "CustomerMembersList" });
  const openMember = (member: FoundMember) => {
    const blade = MEMBER_TYPES[member.memberType as MemberType]?.blade;
    return search.openBlade({
      route: "/customers",
      workspace: "CustomerMembersList",
      // a type without a details blade opens the list (the list says it can't be edited)
      blades: blade ? [{ name: blade, param: member.id }] : [],
    });
  };

  search.registerProvider({
    id: "customers",
    title: "CUSTOMER.SEARCH.GROUP_TITLE",
    icon: "lucide-contact",
    order: 110,
    permission: "customer:read",
    isAvailable: () => search.hasPermission("customer:access"),
    minLength: 2,
    delay: 250,
    aliases: ["customer", "customers", "contact", "contacts", "organization", "organizations"],
    search: async (query: string, context: { take: number; signal: AbortSignal }) => {
      // deepSearch: contacts inside organizations too
      const response = await fetch("/api/members/search", {
        method: "POST",
        signal: context.signal,
        headers: { Accept: "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({ keyword: query, take: context.take, deepSearch: true, sort: "name:asc" }),
      });
      if (!response.ok) throw new Error(`Customers search failed: ${response.status}`);
      const result: { totalCount?: number; results?: FoundMember[] } = await response.json();
      // ranked by the query first (the server sorts by name), then the rest in the server's order
      const members = result.results ?? [];
      const ranked = search.rankItems(members, query, (m) => [m.name, m.fullName, ...(m.emails ?? [])], members.length);
      return {
        total: result.totalCount,
        showAll: async () => {
          await openList();
          requestedKeyword.value = query;
        },
        items: [...ranked, ...members.filter((m) => !ranked.includes(m))].map((member) => ({
          id: member.id,
          title: member.name || member.fullName || member.id,
          description: [member.emails?.[0], member.memberType].filter(Boolean).join(" · "),
          icon: MEMBER_TYPES[member.memberType as MemberType]?.icon ?? "lucide-user",
          execute: () => openMember(member),
        })),
      };
    },
  });
}
