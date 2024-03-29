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
    public class MultiRadioInputModel : BaseModel, IValidatableObject
    {
        protected override IEnumerable<ValidationResult> ExtendedValidation(ValidationContext validationContext)
        {
            if (Mandatory == true)
            {
                if (string.IsNullOrEmpty(DataInput))
                {
                    yield return new ValidationResult($"Enter {CurrentPage.FriendlyFieldName.ToLower()} before continuing", new[] { nameof(DataInput) });
                }
            }
        }

        public override string GetData()
        {
            return DataInput;
        }

    }
}
