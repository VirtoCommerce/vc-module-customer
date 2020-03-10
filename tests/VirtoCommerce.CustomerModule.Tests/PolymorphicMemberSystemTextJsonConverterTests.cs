using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using VirtoCommerce.CustomerModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    public class PolymorphicMemberSystemTextJsonConverterTests
    {
        [Fact]
        public void Serialize()
        {
            var jsonOptions = new JsonSerializerOptions { Converters = { new PolymorphicMemberSystemTextJsonConverter() } };
            var @object = new Contact
            {
                Id = "id",
                Name = "Name1",
                FirstName = "FirstName1",
                MiddleName = "MiddleName1",
                LastName = "LastName1",
                Salutation = "Salutation1",
                FullName = "FullName1",
                BirthDate = new DateTime(2001, 1, 10)
            };

            var result = JsonSerializer.Serialize(@object, jsonOptions);

            Assert.NotNull(result);
        }

        [Fact]
        public void Deserialize()
        {
            var stringObject = "{\"TypeDiscriminator\":\"Contact\",\"TypeValue\":{\"Salutation\":\"Salutation1\",\"FullName\":\"FullName1\",\"FirstName\":\"FirstName1\",\"MiddleName\":\"MiddleName1\",\"LastName\":\"LastName1\",\"BirthDate\":\"2001-01-10T00:00:00\",\"DefaultLanguage\":null,\"TimeZone\":null,\"Organizations\":null,\"TaxPayerId\":null,\"PreferredDelivery\":null,\"PreferredCommunication\":null,\"PhotoUrl\":null,\"ObjectType\":\"VirtoCommerce.CustomerModule.Core.Model.Contact\",\"SecurityAccounts\":[],\"Name\":\"Name1\",\"MemberType\":\"Contact\",\"OuterId\":null,\"Addresses\":null,\"Phones\":null,\"Emails\":null,\"Notes\":null,\"Groups\":null,\"DynamicProperties\":null,\"SeoObjectType\":\"Contact\",\"SeoInfos\":null,\"CreatedDate\":\"0001-01-01T00:00:00\",\"ModifiedDate\":null,\"CreatedBy\":null,\"ModifiedBy\":null,\"ShouldSerializeAuditableProperties\":true,\"Id\":\"id\"}}";
            var jsonOptions = new JsonSerializerOptions { Converters = { new PolymorphicMemberSystemTextJsonConverter() } };

            var result = JsonSerializer.Deserialize<Member>(stringObject, jsonOptions);

            Assert.NotNull(result);
        }
    }

    public class PolymorphicMemberSystemTextJsonConverter : JsonConverter<Member>
    {
        private static readonly Dictionary<string, Type> TypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "Member", typeof(Member) },
            { "Employee", typeof(Employee) },
            { "Contact", typeof(Contact) },
            { "Organization", typeof(Organization) }
        };

        public override bool CanConvert(Type type)
        {
            return TypeMap.Any(t => t.Value.IsAssignableFrom(type));
        }

        public override Member Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName
                || reader.GetString() != "TypeDiscriminator")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            Member result;
            var typeDiscriminator = reader.GetString();
            switch (typeDiscriminator)
            {
                case nameof(Employee):
                    if (!reader.Read() || reader.GetString() != "TypeValue")
                    {
                        throw new JsonException();
                    }
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }
                    result = (Employee)JsonSerializer.Deserialize(ref reader, typeof(Employee));
                    break;
                case nameof(Contact):
                    if (!reader.Read() || reader.GetString() != "TypeValue")
                    {
                        throw new JsonException();
                    }
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }
                    result = (Contact)JsonSerializer.Deserialize(ref reader, typeof(Contact));
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Member value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("TypeDiscriminator", value.MemberType);

            if (value is Employee employee)
            {
                writer.WritePropertyName("TypeValue");
                JsonSerializer.Serialize(writer, employee);
            }
            else if (value is Contact contact)
            {
                writer.WritePropertyName("TypeValue");
                JsonSerializer.Serialize(writer, contact);
            }
            else if (value is Organization organization)
            {
                writer.WritePropertyName("TypeValue");
                JsonSerializer.Serialize(writer, organization);
            }
            else if (value is Vendor vendor)
            {
                writer.WritePropertyName("TypeValue");
                JsonSerializer.Serialize(writer, vendor);
            }
            else
            {
                throw new NotSupportedException();
            }

            writer.WriteEndObject();
        }
    }
}
