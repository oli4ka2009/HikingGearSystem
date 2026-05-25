using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class TripCreateDto : IValidatableObject
    {
        [Required(ErrorMessage = "Назва походу є обов'язковою")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Локація є обов'язковою")]
        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [Range(1, 50, ErrorMessage = "Розмір групи має бути від 1 до 50")]
        public int GroupSize { get; set; }
        public SleepFormat AccommodationFormat { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Дата початку мандрівки не може бути в минулому.",
                    new[] { nameof(StartDate) }
                );
            }

            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Дата завершення мандрівки не може бути раніше дати початку.",
                    new[] { nameof(EndDate) }
                );
            }

            var durationDays = EndDate.DayNumber - StartDate.DayNumber;

            if (durationDays >= 1 && AccommodationFormat == SleepFormat.None)
            {
                yield return new ValidationResult(
                    "Для багатоденних мандрівок необхідно обов'язково вказати формат ночівлі (не може бути 'Без ночівлі').",
                    new[] { nameof(AccommodationFormat) }
                );
            }
        }
    }

    public class TripUpdateDto : IValidatableObject
    {
        [Required(ErrorMessage = "Назва походу є обов'язковою")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Локація є обов'язковою")]
        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [Range(1, 50, ErrorMessage = "Розмір групи має бути від 1 до 50")]
        public int GroupSize { get; set; }

        public SleepFormat AccommodationFormat { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Дата завершення мандрівки не може бути раніше дати початку.",
                    new[] { nameof(EndDate) }
                );
            }

            var durationDays = EndDate.DayNumber - StartDate.DayNumber;

            if (durationDays >= 1 && AccommodationFormat == SleepFormat.None)
            {
                yield return new ValidationResult(
                    "Для багатоденних мандрівок необхідно обов'язково вказати формат ночівлі (не може бути 'Без ночівлі').",
                    new[] { nameof(AccommodationFormat) }
                );
            }
        }
    }

    public class TripResponseDto : TripCreateDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
