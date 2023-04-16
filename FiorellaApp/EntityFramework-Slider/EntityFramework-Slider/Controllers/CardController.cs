using EntityFramework_Slider.Data;
using EntityFramework_Slider.Models;
using EntityFramework_Slider.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EntityFramework_Slider.Controllers
{

    public class CardController : Controller
    {
        private readonly AppDbContext _context;
        public CardController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {

            List<BasketVM> basketProducts;  //bosh bir list yaradiriq

            if (Request.Cookies["basket"] != null) //cookie deki basket null deyilse
            {
                basketProducts = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);    // eger cookide data varsa elimizde olan datani beraber edirik bize gelen teze dataya. bunun uchun ge
                //bunu Listimize deserialize edirikki tipleri beraber olsun
                //eyer coockide data varsa yani null deyilse coockide olan datani goturub = DeserializeObject<List<BasketVM>>.esayn edirik elmizde olan List<BasketVM>e
            }
            else
            {
                basketProducts = new List<BasketVM>();
                //data yoxdursa teze liist yaradir
            }


            List<BasketDetailVM> basketDetails = new();

            foreach (var product in basketProducts)  //elimizdeki basketProducts list weklimdedir deye fora saliriq
            {
                Product dbProduct = await _context.Products.Include(m => m.Images).Include(m => m.Category)?.FirstOrDefaultAsync(m => m.Id == product.Id); // yoxlayiriq databazadaki productun idisinnen cookiedeki productun(yeni basketdeki productun) idsi eynidirse
                                                                                                                                                           //basketDetail deki productdumuzu beraber edirik yoxladigimiz werte. yeni databazadki productnan cookiedeki eyni olan producta

                basketDetails.Add(new BasketDetailVM
                {
                    Id = dbProduct.Id,
                    Name = dbProduct.Name,
                    CategoryName = dbProduct.Category.Name,
                    Description = dbProduct.Description,
                    Price = dbProduct.Price,
                    Image = dbProduct.Images.Where(m => m.IsMain).FirstOrDefault().Image,
                    Count = product.Count,
                    Total = dbProduct.Price * product.Count

                });
            }





            //listimizi gonderirik view a
            return View(basketDetails);
        }






        //Ajax ile reguest geldikde delet methodu iwlesin

        /*  [ActionName("Delete")] */// actionun adi uzun olduqda bele atribut ile adini qisa formada teyin etmek olur.
                                     //delete metodunu ona gore private yazmiriq ki, route-dan bura reguest gelir ,hem actiondur,metod deyil, uidan delete iconuna basanda gelecek bura. private olarsa gelir tapa bilmeyecek  deye public olmalidir
        public IActionResult DeleteProductFromBasket(int? id)
        {
            if (id == null) return BadRequest();

            List<BasketVM> basketProducts = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]); // basketde olan productlarimizi gotururuk

            BasketVM deleteProduct = basketProducts.FirstOrDefault(m => m.Id == id);  // basket productun ichinden delete olunan idli productu tapiriq

            basketProducts.Remove(deleteProduct); // sonra basketdeki productlarimizin ichinden remove edirik silmek isteyeceyimiz  productu

            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketProducts)); // sonra cookie ni(basketi) update edirik. yeniden elimizde olan listi elave edirik cookie ye


            return Ok();
        }


        public IActionResult DecreasetProductCount(int? id)
        {

            if (id == null) return BadRequest();

            List<BasketVM> basketProducts = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);

            BasketVM decreaseProduct = basketProducts.FirstOrDefault(m => m.Id == id);  // basket productun ichinden sayini azaldacagimiz idli productu tapiriq

            if (decreaseProduct.Count > 1)    // sayini azaldacagimiz productun sayi 1den boyukdurse
            {
                var productCount = --decreaseProduct.Count;   //hemin sayini azaldacagimiz productun sayini -- edirik(yeni azaldiriq) ve variableye beraberlewdirib hemin variableni gonderirik response kimi ajax a

                Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketProducts)); //basketi update edirik

                return Ok(productCount);   // sayini azaltdigimiz productu gonderirik

            }



            return Ok();
        }




        public IActionResult IncreaseProductCount(int? id)
        {
            List<BasketVM> basketProducts = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);    // basketde olan productlarimizi gotururuk

            BasketVM? increaseProduct = basketProducts.Find(m => m.Id == id);   // basket productun ichinden sayini artiracagimiz idli productu tapiriq


            var productCount =  ++increaseProduct.Count;   //hemin sayini artiracagimiz productun sayini ++ edirik(yeni artiririq) ve variableye beraberlewdirib gonderirik response kimi ajax a

            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketProducts));   //basket yeni cookie update edilir

           
            return Ok(productCount);   // sayini artiracagimiz productun variablesini gonderirik ajaxa
        }
    }
}
