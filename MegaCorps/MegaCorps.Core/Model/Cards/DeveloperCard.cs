using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Класс карты разработчика
    /// </summary>
    public class DeveloperCard : GameCard
    {
        /// <summary>
        /// Очки развития, которые приносит карта
        /// </summary>
        public int DevelopmentPoint { get; set; }
        public DeveloperCard(int id, int devPoint) : base(id)
        {
            DevelopmentPoint = devPoint;
        }
    }
}
