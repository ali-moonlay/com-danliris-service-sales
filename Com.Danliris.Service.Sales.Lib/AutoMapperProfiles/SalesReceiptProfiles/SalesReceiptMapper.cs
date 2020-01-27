﻿using AutoMapper;
using Com.Danliris.Service.Sales.Lib.Models.SalesReceipt;
using Com.Danliris.Service.Sales.Lib.ViewModels.SalesReceipt;

namespace Com.Danliris.Service.Sales.Lib.AutoMapperProfiles.SalesReceiptProfiles
{
    public class SalesReceiptMapper : Profile
    {
        public SalesReceiptMapper()
        {
            CreateMap<SalesReceiptModel, SalesReceiptViewModel>()
                .ReverseMap();
        }
    }
}
