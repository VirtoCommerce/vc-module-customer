using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public abstract class MemberDataEntity : AuditableEntity
    {
        protected MemberDataEntity()
        {
            Notes = new NullCollection<NoteDataEntity>();
            Addresses = new NullCollection<AddressDataEntity>();
            MemberRelations = new NullCollection<MemberRelationDataEntity>();
            Phones = new NullCollection<PhoneDataEntity>();
            Emails = new NullCollection<EmailDataEntity>();
            Groups = new NullCollection<MemberGroupDataEntity>();
        }

        [StringLength(64)]
        [Index(IsUnique = false)]
        public string MemberType { get; set; }

        [StringLength(128)]
        [Index(IsUnique = false)]
        public string Name { get; set; }

        #region NavigationProperties

        public ObservableCollection<NoteDataEntity> Notes { get; set; }

        public ObservableCollection<AddressDataEntity> Addresses { get; set; }

        public ObservableCollection<MemberRelationDataEntity> MemberRelations { get; set; }

        public ObservableCollection<PhoneDataEntity> Phones { get; set; }

        public ObservableCollection<EmailDataEntity> Emails { get; set; }

        public ObservableCollection<MemberGroupDataEntity> Groups { get; set; }

        #endregion

        public virtual Member ToModel(Member member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            //preserve member type 
            var memberType = member.MemberType;
            member.InjectFrom(this);
            member.MemberType = memberType;

            member.Addresses = Addresses.OrderBy(x => x.Id).Select(x => x.ToModel(AbstractTypeFactory<Address>.TryCreateInstance())).ToList();
            member.Emails = Emails.OrderBy(x => x.Id).Select(x => x.Address).ToList();
            member.Notes = Notes.OrderBy(x => x.Id).Select(x => x.ToModel(new Note())).ToList();
            member.Phones = Phones.OrderBy(x => x.Id).Select(x => x.Number).ToList();
            member.Groups = Groups.OrderBy(x => x.Id).Select(x => x.Group).ToList();

            return member;
        }


        public virtual MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            pkMap.AddPair(member, this);

            this.InjectFrom(member);

            if (member.Phones != null)
            {
                Phones = new ObservableCollection<PhoneDataEntity>();
                foreach (var phone in member.Phones)
                {
                    var phoneEntity = AbstractTypeFactory<PhoneDataEntity>.TryCreateInstance();
                    phoneEntity.Number = phone;
                    phoneEntity.MemberId = member.Id;
                    Phones.Add(phoneEntity);
                }
            }

            if (member.Groups != null)
            {
                Groups = new ObservableCollection<MemberGroupDataEntity>();
                foreach (var group in member.Groups)
                {
                    var groupEntity = AbstractTypeFactory<MemberGroupDataEntity>.TryCreateInstance();
                    groupEntity.Group = group;
                    groupEntity.MemberId = member.Id;
                    Groups.Add(groupEntity);
                }
            }

            if (member.Emails != null)
            {
                Emails = new ObservableCollection<EmailDataEntity>();
                foreach (var email in member.Emails)
                {
                    var emailEntity = AbstractTypeFactory<EmailDataEntity>.TryCreateInstance();
                    emailEntity.Address = email;
                    emailEntity.MemberId = member.Id;
                    Emails.Add(emailEntity);
                }
            }

            if (member.Addresses != null)
            {
                Addresses = new ObservableCollection<AddressDataEntity>(member.Addresses.Select(x => AbstractTypeFactory<AddressDataEntity>.TryCreateInstance().FromModel(x)));
                foreach (var address in Addresses)
                {
                    address.MemberId = member.Id;
                }
            }

            if (member.Notes != null)
            {
                Notes = new ObservableCollection<NoteDataEntity>(member.Notes.Select(x => AbstractTypeFactory<NoteDataEntity>.TryCreateInstance().FromModel(x)));
                foreach (var note in Notes)
                {
                    note.MemberId = member.Id;
                }
            }
            return this;
        }


        public virtual void Patch(MemberDataEntity target)
        {
            target.Name = Name;

            if (!Phones.IsNullCollection())
            {
                var phoneComparer = AnonymousComparer.Create((PhoneDataEntity x) => x.Number);
                Phones.Patch(target.Phones, phoneComparer, (sourcePhone, targetPhone) => targetPhone.Number = sourcePhone.Number);
            }

            if (!Emails.IsNullCollection())
            {
                var addressComparer = AnonymousComparer.Create((EmailDataEntity x) => x.Address);
                Emails.Patch(target.Emails, addressComparer, (sourceEmail, targetEmail) => targetEmail.Address = sourceEmail.Address);
            }

            if (!Groups.IsNullCollection())
            {
                var groupComparer = AnonymousComparer.Create((MemberGroupDataEntity x) => x.Group);
                Groups.Patch(target.Groups, groupComparer, (sourceGroup, targetGroup) => targetGroup.Group = sourceGroup.Group);
            }

            if (!Addresses.IsNullCollection())
            {
                Addresses.Patch(target.Addresses, (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }

            if (!Notes.IsNullCollection())
            {
                var noteComparer = AnonymousComparer.Create((NoteDataEntity x) => x.Id ?? x.Body);
                Notes.Patch(target.Notes, noteComparer, (sourceNote, targetNote) => sourceNote.Patch(targetNote));
            }

            if (!MemberRelations.IsNullCollection())
            {
                var relationComparer = AnonymousComparer.Create((MemberRelationDataEntity x) => x.AncestorId);
                MemberRelations.Patch(target.MemberRelations, relationComparer, (sourceRel, targetRel) => { /*Nothing todo*/ });
            }
        }
    }
}
