using Corps.Server.Hubs;
using Corps.Server.Utils.JSON;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
public class Image
{
    public ImageType Type { get; set; }
    public byte[] ImageData { get; set; }
}

public enum ImageType
{
    Menu,
    Board,
    CardBackground,
    CardIcon,
}

public class CardImage
{
    public CardImage(AttackCard attackCard)
    {
        Id = attackCard.Id;
    }

    public CardImage(DefenceCard defenceCard)
    {
        Id = defenceCard.Id;
    }

    public CardImage(DeveloperCard developerCard)
    {
        Id = developerCard.Id;
    }

    public int Id { get; set; }
    public Image Background { get; set; }
    public Image Icon { get; set; }
    public CardDirectionDTO Direction { get; set; }
    public CardInfo Info { get; set; }
}

public class CardInfo
{
    [JsonProperty("attack_type")]
    public AttackType AttackType{ get; set; }
    public string Title { get; set; }
    public string Description{ get; set; }
}

public class CardDirectionDTO
{
    [JsonProperty("card_direction")]
    public CardDirection Direction { get; set; }
}

namespace Corps.Server.Services
{
    public class ImageService
    {
        Dictionary<(AttackType,CardDirection),CardInfo> attackTypes = new Dictionary<(AttackType, CardDirection), CardInfo>();
        List<CardDirectionDTO> directions = new List<CardDirectionDTO>();
        List<CardInfo> infos = new List<CardInfo>();

        public ImageService()
        {
            using (StreamReader r = new StreamReader("..\\Resource\\Text\\Card\\Direction\\directions.json"))
            {
                string json = r.ReadToEnd();
                directions = DataSerializer.Deserialize<List<CardDirectionDTO>>(json);
            }
            using (StreamReader r = new StreamReader("..\\Resource\\Text\\Card\\Description\\descriptions.json"))
            {
                string json = r.ReadToEnd();
                infos = DataSerializer.Deserialize<List<CardInfo>>(json);
            }
        }

        public async Task SendImagesToClients(List<Image> images,IClientProxy group)
        {
            await group.SendAsync("ReceiveImages", images.Select(bg => bg.ImageData).ToList());
        }
        public async Task SendCardsToClients(List<GameCard> cards, IClientProxy group)
        {
            List<CardImage> images = new List<CardImage>();
            cards.ForEach(x => {
                if(x is AttackCard)
                {
                    images.Add(new CardImage((x as AttackCard)!));
                }
                else if (x is DefenceCard)
                {
                    images.Add(new CardImage((x as DefenceCard)!));
                }
                else if (x is DeveloperCard)
                {
                    images.Add(new CardImage((x as DeveloperCard)!));
                }
            });
            await group.SendAsync("ReceiveCards", images);
        }
        private string GetCardDataFromFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

                foreach (var imagePath in imageFiles)
                {
                    MapData map = new MapData
                    {
                        BackgroundImage = LoadImage(imagePath),
                        IconImage = LoadIconImage(imagePath), // Логика для загрузки иконки изображения
                        Text = Path.GetFileNameWithoutExtension(imagePath)
                    };

                    mapList.Add(map);
                }
            }
            else
            {
                Console.WriteLine("Папка не найдена.");
            }

            return mapList;
        }

        // Метод для объединения массивов байтов
        private static byte[] ConcatenateByteArrays(params byte[][] arrays)
        {
            int totalLength = arrays.Sum(arr => arr.Length);
            byte[] result = new byte[totalLength];
            int offset = 0;

            foreach (byte[] arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }
    }
}
