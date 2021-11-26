using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class WebBankingController : Controller
    {
        // Punto - 3
        [HttpGet]
        [Route("conti-correnti")]
        public ActionResult GetBankAccount()
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                if(stato)
                {
                    return Ok(model.BankAccounts.ToList());
                }
                else 
                {
                    string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                    int idUser = model.Users.Where(w => w.Username == nameUser).Select(s => s.Id).FirstOrDefault();
                    List<BankAccount> myBankAccount = model.BankAccounts.Where(w => w.FkUser == idUser).ToList();
                    if (myBankAccount != null)
                        return Ok(myBankAccount);
                    else
                        return NotFound("L'utente loggato non ha conti bancari");
                }

            }

        }

        // Punto - 4
        [HttpGet]
        [Route("conti-correnti/{id}")]
        public ActionResult GetOneBankAccount(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).Select(s => s.FkUser).FirstOrDefault();
                int idUser = model.Users.Where(w => w.Username == nameUser).Select(s => s.Id).FirstOrDefault();
                if (idBankFkUser == null)
                {
                    return NotFound("L'id del conto bancario selezionato non è presente.");
                }
                else if (idBankFkUser == idUser || stato)
                {
                    BankAccount myBank = model.BankAccounts.Where(w => w.Id == id).FirstOrDefault();
                    return Ok(myBank);
                }
                else
                    return Unauthorized("Il conto bancario selezionato non è accessibile dall'utente loggato.");
            }

        }

        // Punto - 5
        [HttpGet]
        [Route("conti-correnti/{id}/movimenti")]
        public ActionResult GetMovement(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).Select(s => s.FkUser).FirstOrDefault();
                int idUser = model.Users.Where(w => w.Username == nameUser).Select(s => s.Id).FirstOrDefault();
                if (idBankFkUser == null)
                {
                    return NotFound("L'id del conto bancario selezionato non è presente.");
                }
                else if (idBankFkUser == idUser || stato)
                {
                    List<AccountMovement> myMovement = model.AccountMovements.Where(w => w.FkBankAccount == id).ToList();

                    if(myMovement != null)
                        return Ok(myMovement);
                    else
                        return NotFound("Non sono presenti movimenti per questo conto.");
                }
                else
                    return Unauthorized("I movimenti del conto bancario selezionato non sono accessibili dall'utente loggato.");
            }

        }

        // Punto - 6
        [HttpGet]
        [Route("conti-correnti/{id}/movimenti/{idMovement}")]
        public ActionResult GetOneMovement(int id, int idMovement)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).Select(s => s.FkUser).FirstOrDefault();
                var verifyMovement = model.AccountMovements.Where(w => w.Id == idMovement).FirstOrDefault();
                int idUser = model.Users.Where(w => w.Username == nameUser).Select(s => s.Id).FirstOrDefault();
                if (idBankFkUser == null)
                {
                    return NotFound("L'id del conto bancario selezionato non è presente.");
                }
                else if (verifyMovement == null)
                {
                    return NotFound("L'id del movimento selezionato non esiste.");
                }
                else if (idBankFkUser == idUser || stato)
                {
                    List<AccountMovement> myMovement = model.AccountMovements.Where(w => w.FkBankAccount == id).ToList();

                    if(myMovement != null)
                    {
                        AccountMovement mySingleMovement = null;
                        foreach (var m in myMovement)
                        {

                            if (m.Id == idMovement)
                                mySingleMovement = m;
                        }
                        if (mySingleMovement == null)
                            return Unauthorized("L'id del movimento selezionato non è presente tra i movimenti di questo conto.");
                        else
                            return Ok(mySingleMovement);
                    }
                    else
                        return Problem("Non sono presenti movimenti associati a questo conto bancario.");
                }
                else
                    return Unauthorized("Il movimento selezionato del conto bancario non è accessibile dall'utente loggato.");
            }

        }

        // Punto - 7
        [HttpPost("")]
        [Route("conti-correnti/{id}/bonifico")]
        public ActionResult Transfer(int id, [FromBody] MoneyTransfer bonifico)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).Select(s => s.FkUser).FirstOrDefault();
                int idUser = model.Users.Where(w => w.Username == nameUser).Select(s => s.Id).FirstOrDefault();
                if (idBankFkUser == null)
                {
                    return NotFound("L'id del conto bancario selezionato non è presente.");
                }
                else if (idBankFkUser == idUser || stato)
                {
                    BankAccount toAccount = model.BankAccounts.Where(w => w.Iban == bonifico.Iban).FirstOrDefault();

                    if(bonifico.Importo <= 0)
                    {
                        return Problem("L'importo inserito non è valido");
                    }
                    else if(toAccount != null)
                    {
                        try
                        {
                            AccountMovement movementOut = new AccountMovement(DateTime.Now, id, null, bonifico.Importo, "");
                            AccountMovement movementIn = new AccountMovement(DateTime.Now, toAccount.Id, bonifico.Importo, null, "");
                            model.AccountMovements.Add(movementOut);
                            model.AccountMovements.Add(movementIn);
                            model.SaveChanges();
                            return Ok("Denaro inviato correttamente");
                        }
                        catch (Exception)
                        {
                            return Problem("Inserire correttamente tutti i campi");
                        }
                    }
                    else
                    {
                        return NotFound("L'iban selezionato non è presente.");
                    }
                    
                    
                }
                else
                    return Unauthorized("Il conto bancario selezionato non è accessibile dall'utente loggato.");
            }

        }

        // Punto - 8
        [HttpPost("")]
        [Route("conti-correnti")]
        public ActionResult CreaConto([FromBody] BankAccount nuovoConto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                
                if (stato)
                {
                    User candidate = model.Users.FirstOrDefault(q => q.Id == nuovoConto.FkUser);

                    if (candidate != null)
                    {
                        var existIban = model.BankAccounts.Where(w => w.Iban == nuovoConto.Iban).FirstOrDefault();

                        if(existIban == null)
                        {
                            try
                            {

                                model.BankAccounts.Add(nuovoConto);
                                model.SaveChanges();
                                return Ok("Conto bancario creato correttamente.");
                            }
                            catch (Exception)
                            {
                                return Problem("Inserire correttamente tutti i campi");
                            }
                        }
                        else
                        {
                            return Problem("L'iban inserito è già presente.");
                        }
                    }
                    else
                    {
                        return Problem("La fkUser inserita non esiste");
                    }


                }
                else
                    return Unauthorized("L'utente selezionato non ha i privilegi per creare un conto bancario.");
            }

        }

        // Punto - 9
        [HttpPost("")]
        [Route("conti-correnti/{id}")]
        public ActionResult AggiornaConto(int id, [FromBody] BankAccount nuovoConto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);
                string nameUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).FirstOrDefault();
                var idFkUser = model.Users.Where(w => w.Id == nuovoConto.FkUser).FirstOrDefault();
                var searchIban = model.BankAccounts.Where(w => w.Iban == nuovoConto.Iban).FirstOrDefault();

                if (idBankFkUser != null)
                {
                    return Problem("L'iban inserito è già presente tra i conti.");
                }
                if (idBankFkUser == null)
                {
                    return Problem("L'id del conto bancario selezionato non è presente.");
                }
                else if (idBankFkUser == null)
                {
                    return Problem("L'FkUser inserito non esiste.");
                }
                else if (stato)
                {
                    User candidate = model.Users.FirstOrDefault(q => q.Id == nuovoConto.FkUser);

                    if (candidate != null)
                    {
                        var existIban = model.BankAccounts.Where(w => w.Iban == nuovoConto.Iban).FirstOrDefault();

                        if (existIban == null)
                        {
                            try
                            {
                                BankAccount myBank = model.BankAccounts.Where(w => w.Id == id).FirstOrDefault();

                                myBank.Iban = nuovoConto.Iban;
                                myBank.FkUser = nuovoConto.FkUser;
                                model.SaveChanges();
                                return Ok("Conto bancario aggiornato correttamente.");
                            }
                            catch (Exception)
                            {
                                return Problem("Inserire correttamente tutti i campi");
                            }
                        }
                        else
                        {
                            return Problem("L'iban inserito è già presente.");
                        }
                    }
                    else
                    {
                        return Problem("La fkUser inserita non esiste");
                    }


                }
                else
                    return Unauthorized("L'utente selezionato non ha i privilegi per aggiornare un conto bancario.");
            }

        }

        [HttpDelete("conti-correnti/{id}")] //Delete
        public ActionResult Delete(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                bool stato = bool.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "state").Value);

                var idBankFkUser = model.BankAccounts.Where(w => w.Id == id).FirstOrDefault();
                if (idBankFkUser == null)
                {
                    return NotFound("L'id del conto bancario selezionato non è presente.");
                }
                else if (stato)
                {
                    try
                    {
                        BankAccount myBank = model.BankAccounts.Where(w => w.Id == id).FirstOrDefault();
                        List <AccountMovement> movPres = model.AccountMovements.Where(w => w.FkBankAccount == id).ToList();
                        foreach (var i in movPres)
                            model.AccountMovements.Remove(i);
                        model.BankAccounts.Remove(myBank);
                        model.SaveChanges();
                        return Ok("Conto bancario eliminato correttamente.");
                    }
                    catch (Exception)
                    {
                        return Problem("Inserire correttamente tutti i campi");
                    }
                }
                else
                    return Unauthorized("L'utente loggato non ha i privilegi per eliminare un conto bancario.");
            }
        }

    }
}
