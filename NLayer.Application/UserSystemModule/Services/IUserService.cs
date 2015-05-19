﻿using System;
using NLayer.Application.UserSystemModule.DTOs;
using PagedList;

namespace NLayer.Application.UserSystemModule.Services
{
    public interface IUserService
    {
        UserDTO Add(UserDTO userDTO);

        void Update(UserDTO userDTO);

        void Remove(Guid id);

        UserDTO FindBy(Guid id);
 
        IPagedList<UserDTO> FindBy(string name, int pageNumber, int pageSize);
    }
}
