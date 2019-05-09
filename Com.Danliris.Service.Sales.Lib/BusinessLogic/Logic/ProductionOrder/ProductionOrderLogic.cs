﻿using Com.Danliris.Service.Sales.Lib.Models.ProductionOrder;
using Com.Danliris.Service.Sales.Lib.Services;
using Com.Danliris.Service.Sales.Lib.Utilities;
using Com.Danliris.Service.Sales.Lib.Utilities.BaseClass;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.ProductionOrder
{
    public class ProductionOrderLogic : BaseLogic<ProductionOrderModel>
    {
        private ProductionOrder_DetailLogic productionOrder_DetailLogic;
        private ProductionOrder_LampStandardLogic productionOrder_LampStandardLogic;
        private ProductionOrder_RunWidthLogic productionOrder_RunWidthLogic;

        public ProductionOrderLogic(IServiceProvider serviceProvider, IIdentityService identityService, SalesDbContext dbContext) : base(identityService, serviceProvider, dbContext)
        {
            this.productionOrder_DetailLogic = serviceProvider.GetService<ProductionOrder_DetailLogic>();
            this.productionOrder_LampStandardLogic = serviceProvider.GetService<ProductionOrder_LampStandardLogic>();
            this.productionOrder_RunWidthLogic = serviceProvider.GetService<ProductionOrder_RunWidthLogic>();

        }
        public override ReadResponse<ProductionOrderModel> Read(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            IQueryable<ProductionOrderModel> Query = DbSet;

            List<string> SearchAttributes = new List<string>()
            {
              "OrderNo", "SalesContractNo", "BuyerType", "BuyerName", "ProcessTypeName"
            };

            Query = QueryHelper<ProductionOrderModel>.Search(Query, SearchAttributes, keyword);

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            Query = QueryHelper<ProductionOrderModel>.Filter(Query, FilterDictionary);

            List<string> SelectedFields = new List<string>()
            {

                "Id", "Code", "Buyer", "ProcessType", "LastModifiedUtc", "FinishingPrintingSalesContract", "OrderNo", "Details", "OrderType", "HandlingStandard", "Material", "YarnMaterial", "DeliveryDate", "SalesContractNo", "MaterialConstruction", "FinishWidth", "DesignCode", "DesignNumber"

            };

            Query = Query
                .Select(field => new ProductionOrderModel
                {
                    Id = field.Id,
                    Code = field.Code,
                    DeliveryDate = field.DeliveryDate,
                    HandlingStandard = field.HandlingStandard,
                    FinishWidth = field.FinishWidth,
                    MaterialId = field.MaterialId,
                    MaterialCode = field.MaterialCode,
                    MaterialName = field.MaterialName,
                    MaterialConstructionId = field.MaterialConstructionId,
                    MaterialConstructionCode = field.MaterialConstructionCode,
                    MaterialConstructionName = field.MaterialConstructionName,
                    SalesContractNo = field.SalesContractNo,
                    BuyerType = field.BuyerType,
                    BuyerName = field.BuyerName,
                    BuyerId = field.BuyerId,
                    OrderNo = field.OrderNo,
                    ProcessTypeId = field.ProcessTypeId,
                    ProcessTypeCode = field.ProcessTypeCode,
                    ProcessTypeName = field.ProcessTypeName,

                    YarnMaterialId = field.YarnMaterialId,
                    YarnMaterialCode = field.YarnMaterialCode,
                    YarnMaterialName = field.YarnMaterialName,
                    OrderTypeId = field.OrderTypeId,
                    OrderTypeCode = field.OrderTypeCode,
                    OrderTypeName = field.OrderTypeName,
                    LastModifiedUtc = field.LastModifiedUtc,
                    Details = field.Details,
                    DesignCode = field.DesignCode,
                    DesignNumber = field.DesignNumber
                });

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            Query = QueryHelper<ProductionOrderModel>.Order(Query, OrderDictionary);

            Pageable<ProductionOrderModel> pageable = new Pageable<ProductionOrderModel>(Query, page - 1, size);
            List<ProductionOrderModel> data = pageable.Data.ToList<ProductionOrderModel>();
            int totalData = pageable.TotalCount;

            return new ReadResponse<ProductionOrderModel>(data, totalData, OrderDictionary, SelectedFields);
        }

        public override async void UpdateAsync(long id, ProductionOrderModel model)
        {
            try
            {
                if (model.Details != null)
                {
                    HashSet<long> detailIds = productionOrder_DetailLogic.GetIds(id);
                    foreach (var itemId in detailIds)
                    {
                        ProductionOrder_DetailModel data = model.Details.FirstOrDefault(prop => prop.Id.Equals(itemId));
                        if (data == null)
                            await productionOrder_DetailLogic.DeleteAsync(itemId);
                        else
                        {
                            productionOrder_DetailLogic.UpdateAsync(itemId, data);
                        }

                        foreach (ProductionOrder_DetailModel item in model.Details)
                        {
                            if (item.Id == 0)
                                productionOrder_DetailLogic.Create(item);
                        }
                    }

                    HashSet<long> LampStandardIds = productionOrder_LampStandardLogic.GetIds(id);
                    foreach (var itemId in LampStandardIds)
                    {
                        ProductionOrder_LampStandardModel data = model.LampStandards.FirstOrDefault(prop => prop.Id.Equals(itemId));
                        if (data == null)
                            await productionOrder_LampStandardLogic.DeleteAsync(itemId);
                        else
                        {
                            productionOrder_LampStandardLogic.UpdateAsync(itemId, data);
                        }

                        foreach (ProductionOrder_LampStandardModel item in model.LampStandards)
                        {
                            if (item.Id == 0)
                                productionOrder_LampStandardLogic.Create(item);
                        }
                    }

                    if (model.RunWidths.Count > 0)
                    {
                        HashSet<long> RunWidthIds = productionOrder_RunWidthLogic.GetIds(id);
                        foreach (var itemId in RunWidthIds)
                        {
                            ProductionOrder_RunWidthModel data = model.RunWidths.FirstOrDefault(prop => prop.Id.Equals(itemId));
                            if (data == null)
                               await productionOrder_RunWidthLogic.DeleteAsync(itemId);
                            else
                            {
                                productionOrder_RunWidthLogic.UpdateAsync(itemId, data);
                            }

                            foreach (ProductionOrder_RunWidthModel item in model.RunWidths)
                            {
                                if (item.Id == 0)
                                    productionOrder_RunWidthLogic.Create(item);
                            }
                        }
                    }

                }

                EntityExtension.FlagForUpdate(model, IdentityService.Username, "sales-service");
                DbSet.Update(model);
            }catch(Exception ex)
            {
                throw ex;
            }
            

        }


        public override void Create(ProductionOrderModel model)
        {
            if (model.Details.Count > 0)
            {
                foreach (var detail in model.Details)
                {
                    productionOrder_DetailLogic.Create(detail);
                }
            }

            if (model.LampStandards.Count > 0)
            {
                foreach (var lampStandards in model.LampStandards)
                {
                    lampStandards.Id = 0;
                    productionOrder_LampStandardLogic.Create(lampStandards);
                }
            }

            if (model.RunWidths.Count > 0)
            {
                foreach (var runWidths in model.RunWidths)
                {
                    productionOrder_RunWidthLogic.Create(runWidths);
                }
            }

            EntityExtension.FlagForCreate(model, IdentityService.Username, "sales-service");
            DbSet.Add(model);
        }

        public override async Task DeleteAsync(long id)
        {

            ProductionOrderModel model = await ReadByIdAsync(id);

            foreach (var detail in model.Details)
            {
                await productionOrder_DetailLogic.DeleteAsync(detail.Id);
            }

            foreach (var lampStandards in model.LampStandards)
            {
                await productionOrder_LampStandardLogic.DeleteAsync(lampStandards.Id);
            }


            if (model.RunWidths.Count > 0)
            {
                foreach (var runWidths in model.RunWidths)
                {
                    await productionOrder_RunWidthLogic.DeleteAsync(runWidths.Id);
                }
            }

            EntityExtension.FlagForDelete(model, IdentityService.Username, "sales-service", true);
            DbSet.Update(model);
        }

        public override async Task<ProductionOrderModel> ReadByIdAsync(long id)
        {
            var ProductionOrder = await DbSet.Where(p=>p.Details.Select(d=>d.ProductionOrderModel.Id).Contains(p.Id)).Include(p => p.Details)
                .Include(p => p.LampStandards).Include(p => p.RunWidths).FirstOrDefaultAsync(d => d.Id.Equals(id) && d.IsDeleted.Equals(false));
            ProductionOrder.Details = ProductionOrder.Details.OrderBy(s => s.Id).ToArray();
            ProductionOrder.LampStandards = ProductionOrder.LampStandards.OrderBy(s => s.Id).ToArray();
            ProductionOrder.RunWidths = ProductionOrder.RunWidths.OrderBy(s => s.Id).ToArray();
            return ProductionOrder;
        }
    }
}
