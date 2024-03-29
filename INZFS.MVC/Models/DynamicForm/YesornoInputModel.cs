﻿using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models.DynamicForm
{
    public class YesornoInputModel : BaseModel, IValidatableObject
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true && MarkAsComplete)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult($"Select {CurrentPage.FriendlyFieldName.ToLower()} before continuing", new[] { nameof(DataInput) });
                }
            }
        }

        public override string GetData()
        {
            return DataInput;
        }

    }
}
