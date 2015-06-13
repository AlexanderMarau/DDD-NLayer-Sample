﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLayer.Application.Exceptions;
using NLayer.Application.Resources;
using NLayer.Application.UserSystemModule.Converters;
using NLayer.Application.UserSystemModule.DTOs;
using NLayer.Domain.UserSystemModule.Aggregates.MenuAgg;
using NLayer.Domain.UserSystemModule.Aggregates.UserAgg;
using NLayer.Infrastructure.Entity;
using NLayer.Infrastructure.Utility.Helper;
using PagedList;

namespace NLayer.Application.UserSystemModule.Services
{
    public class UserService : IUserService
    {
        IUserRepository _Repository;
        IPermissionRepository _PermissionRepository;

        #region Constructors

        public UserService(IUserRepository repository, IPermissionRepository permissionRepository)                               
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            _Repository = repository;
            _PermissionRepository = permissionRepository;
        }

        #endregion

        public UserDTO Add(UserDTO userDTO)
        {
            var user = userDTO.ToModel();
            user.Id = IdentityGenerator.NewSequentialGuid();
            user.Created = DateTime.UtcNow;
            user.LastLogin = Const.SqlServerNullDateTime;

            if (user.Name.IsNullOrBlank())
            {
                throw new DataExistsException(UserSystemResource.Common_Name_Empty);
            }

            if (_Repository.Exists(user))
            {
                throw new DataExistsException(UserSystemResource.User_Exists);
            }

            user.LoginPwd = AuthService.EncryptPassword(user.LoginPwd);
            _Repository.Add(user);

            //commit the unit of work
            _Repository.UnitOfWork.Commit();

            return user.ToDto();
        }

        public void Update(UserDTO userDTO)
        {
            //get persisted item
            var persisted = _Repository.Get(userDTO.Id);

            if (persisted != null) //if customer exist
            {
                var current = userDTO.ToModel();
                current.Created = persisted.Created;    //不修改创建时间
                current.LoginPwd = persisted.LoginPwd;    //不修改密码

                if (current.Name.IsNullOrBlank())
                {
                    throw new DataExistsException(UserSystemResource.Common_Name_Empty);
                }

                if (_Repository.Exists(current))
                {
                    throw new DataExistsException(UserSystemResource.User_Exists);
                }

                //Merge changes
                _Repository.Merge(persisted, current);

                //commit unit of work
                _Repository.UnitOfWork.Commit();
            }
            else
            {
                throw new DataNotFoundException(UserSystemResource.User_NotExists);
            }
        }

        public void Remove(Guid id)
        {
            var user = _Repository.Get(id);

            if (user != null) //if exist
            {
                _Repository.Remove(user);

                //commit unit of work
                _Repository.UnitOfWork.Commit();
            }
            else
            {
                // Not Exists
            }
        }
        public UserDTO FindBy(Guid id)
        {
            return _Repository.Get(id).ToDto();
        }


        public IPagedList<UserDTO> FindBy(string name, int pageNumber, int pageSize)
        {
            var list = _Repository.FindBy(name, pageNumber, pageSize);
            return new StaticPagedList<UserDTO>(
               list.ToList().Select(x => x.ToDto()),
               pageNumber,
               pageSize,
               list.TotalItemCount);
        }

        public void UpdateUserPermission(Guid id, List<Guid> permissions)
        {
            //get persisted item
            var persisted = _Repository.Get(id);

            if (persisted != null) //if customer exist
            {
                var pList = new List<Permission>();
                foreach (var pid in permissions)
                {
                    var p = _PermissionRepository.Get(pid);
                    if (p != null)
                    {
                        pList.Add(p);
                    }
                }

                // 删除旧的权限
                persisted.Permissions.Clear();
                // 添加新的权限
                persisted.Permissions = pList;

                //commit unit of work
                _Repository.UnitOfWork.Commit();
            }
        }

        public List<PermissionDTO> GetUserPermission(Guid id)
        {
            //get persisted item
            var persisted = _Repository.Get(id);

            if (persisted != null) //if customer exist
            {
                return persisted.Permissions.Select(x => x.ToDto()).ToList();
            }

            return new List<PermissionDTO>();
        }
    }
}
