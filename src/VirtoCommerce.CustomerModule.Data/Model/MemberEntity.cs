using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public abstract class MemberEntity : AuditableEntity, IHasOuterId
    {
        [StringLength(64)]
        public string MemberType { get; set; }

        [StringLength(512)]
        public string Name { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        [StringLength(64)]
        public string Status { get; set; }

        [StringLength(1024)]
        public string IconUrl { get; set; }

        #region NavigationProperties

        public virtual ObservableCollection<NoteEntity> Notes { get; set; } = new NullCollection<NoteEntity>();

        public virtual ObservableCollection<AddressEntity> Addresses { get; set; } = new NullCollection<AddressEntity>();

        public virtual ObservableCollection<MemberRelationEntity> MemberRelations { get; set; } = new NullCollection<MemberRelationEntity>();

        public virtual ObservableCollection<PhoneEntity> Phones { get; set; } = new NullCollection<PhoneEntity>();

        public virtual ObservableCollection<EmailEntity> Emails { get; set; } = new NullCollection<EmailEntity>();

        public virtual ObservableCollection<MemberGroupEntity> Groups { get; set; } = new NullCollection<MemberGroupEntity>();

        public virtual ObservableCollection<SeoInfoEntity> SeoInfos { get; set; } = new NullCollection<SeoInfoEntity>();

        public virtual ObservableCollection<MemberDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<MemberDynamicPropertyObjectValueEntity>();

        #endregion NavigationProperties

        public virtual Member ToModel(Member member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            member.Id = Id;
            member.CreatedBy = CreatedBy;
            member.CreatedDate = CreatedDate;
            member.ModifiedBy = ModifiedBy;
            member.ModifiedDate = ModifiedDate;
            member.MemberType = MemberType;
            member.Name = Name;
            member.OuterId = OuterId;
            member.Status = Status;
            member.IconUrl = IconUrl;

            member.Addresses = Addresses.OrderBy(x => x.Id).Select(x => x.ToModel(AbstractTypeFactory<Address>.TryCreateInstance())).ToList();
            member.Emails = Emails.OrderBy(x => x.Id).Select(x => x.Address).ToList();
            member.Notes = Notes.OrderBy(x => x.Id).Select(x => x.ToModel(new Note())).ToList();
            member.Phones = Phones.OrderBy(x => x.Id).Select(x => x.Number).ToList();
            member.Groups = Groups.OrderBy(x => x.Id).Select(x => x.Group).ToList();
            member.SeoInfos = SeoInfos.Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();

            member.DynamicProperties = DynamicPropertyObjectValues.GroupBy(x => x.PropertyId ?? x.PropertyName) // PropertyId is null for formally non-defined properties, therefore use the name for grouping
                .Select(x =>
              {
                  var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                  property.Id = x.First()?.PropertyId;
                  property.Name = x.FirstOrDefault()?.PropertyName;
                  property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToList();
                  return property;
              }).ToList();

            return member;
        }

        public virtual MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            pkMap.AddPair(member, this);

            Id = member.Id;
            CreatedBy = member.CreatedBy;
            CreatedDate = member.CreatedDate;
            ModifiedBy = member.ModifiedBy;
            ModifiedDate = member.ModifiedDate;
            MemberType = member.MemberType;
            Name = member.Name;
            OuterId = member.OuterId;
            Status = member.Status;
            IconUrl = member.IconUrl;

            if (member.Phones != null)
            {
                Phones = new ObservableCollection<PhoneEntity>();
                foreach (var phone in member.Phones)
                {
                    var phoneEntity = AbstractTypeFactory<PhoneEntity>.TryCreateInstance();
                    phoneEntity.Number = phone;
                    phoneEntity.MemberId = member.Id;
                    Phones.Add(phoneEntity);
                }
            }

            if (member.Groups != null)
            {
                Groups = new ObservableCollection<MemberGroupEntity>();
                foreach (var group in member.Groups)
                {
                    var groupEntity = AbstractTypeFactory<MemberGroupEntity>.TryCreateInstance();
                    groupEntity.Group = group;
                    groupEntity.MemberId = member.Id;
                    Groups.Add(groupEntity);
                }
            }

            if (member.Emails != null)
            {
                Emails = new ObservableCollection<EmailEntity>();
                foreach (var email in member.Emails)
                {
                    var emailEntity = AbstractTypeFactory<EmailEntity>.TryCreateInstance();
                    emailEntity.Address = email;
                    emailEntity.MemberId = member.Id;
                    Emails.Add(emailEntity);
                }
            }

            if (member.Addresses != null)
            {
                Addresses = new ObservableCollection<AddressEntity>(member.Addresses.Select(x => AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(x)));
                foreach (var address in Addresses)
                {
                    address.MemberId = member.Id;
                }
            }

            if (member.Notes != null)
            {
                Notes = new ObservableCollection<NoteEntity>(member.Notes.Select(x => AbstractTypeFactory<NoteEntity>.TryCreateInstance().FromModel(x)));
                foreach (var note in Notes)
                {
                    note.MemberId = member.Id;
                }
            }

            if (member.SeoInfos != null)
            {
                SeoInfos = new ObservableCollection<SeoInfoEntity>(member.SeoInfos.Select(x => AbstractTypeFactory<SeoInfoEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (member.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<MemberDynamicPropertyObjectValueEntity>(member.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<MemberDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, member, p))).OfType<MemberDynamicPropertyObjectValueEntity>());
            }

            return this;
        }

        public virtual void Patch(MemberEntity target)
        {
            target.Name = Name;
            target.MemberType = MemberType;
            target.OuterId = OuterId;
            target.Status = Status;
            target.IconUrl = IconUrl;

            if (!Phones.IsNullCollection())
            {
                PatchPhones(target);
            }

            if (!Emails.IsNullCollection())
            {
                PatchEmails(target);
            }

            if (!Groups.IsNullCollection())
            {
                PatchGroups(target);
            }

            if (!Notes.IsNullCollection())
            {
                PatchNotes(target);
            }

            if (!MemberRelations.IsNullCollection())
            {
                PatchMemberRelations(target);
            }

            if (!Addresses.IsNullCollection())
            {
                Addresses.Patch(target.Addresses, (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }

            if (!SeoInfos.IsNullCollection())
            {
                SeoInfos.Patch(target.SeoInfos, (sourceSeoInfo, targetSeoInfo) => sourceSeoInfo.Patch(targetSeoInfo));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(target.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }

        public virtual void PatchPhones(MemberEntity target)
        {
            var phoneComparer = AnonymousComparer.Create((PhoneEntity x) => x.Number);
            Phones.Patch(target.Phones, phoneComparer, (sourcePhone, targetPhone) => targetPhone.Number = sourcePhone.Number);
        }

        public virtual void PatchEmails(MemberEntity target)
        {
            var addressComparer = AnonymousComparer.Create((EmailEntity x) => x.Address);
            Emails.Patch(target.Emails, addressComparer, (sourceEmail, targetEmail) => targetEmail.Address = sourceEmail.Address);
        }

        public virtual void PatchGroups(MemberEntity target)
        {
            var groupComparer = AnonymousComparer.Create((MemberGroupEntity x) => x.Group);
            Groups.Patch(target.Groups, groupComparer, (sourceGroup, targetGroup) => targetGroup.Group = sourceGroup.Group);
        }

        public virtual void PatchNotes(MemberEntity target)
        {
            var noteComparer = AnonymousComparer.Create((NoteEntity x) => x.Id ?? x.Body);
            Notes.Patch(target.Notes, noteComparer, (sourceNote, targetNote) => sourceNote.Patch(targetNote));
        }

        public virtual void PatchMemberRelations(MemberEntity target)
        {
            var relationComparer = AnonymousComparer.Create((MemberRelationEntity x) => x.AncestorId);
            MemberRelations.Patch(target.MemberRelations, relationComparer, (sourceRel, targetRel) => { /*Nothing todo*/ });
        }
    }
}
