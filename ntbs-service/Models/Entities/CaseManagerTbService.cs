﻿using ntbs_service.Models.ReferenceEntities;

namespace ntbs_service.Models.Entities
{
    public class CaseManagerTbService
    {
        public string TbServiceCode { get; set; }
        public virtual TBService TbService { get; set; }

        public string CaseManagerUsername { get; set; }
        public virtual User CaseManager { get; set; }
    }
}
