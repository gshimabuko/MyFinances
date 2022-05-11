using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyFinances.Data;
using MyFinances.Models;

namespace MyFinances.Controllers
{
    [Authorize]
    public class TransfersController : Controller
    {
        private MyFinancesContext db = new MyFinancesContext();

        // GET: Transfers
        public ActionResult Index()
        {
            var transfers = db.Transfers.Include(t => t.destinationAccount).Include(t => t.sourceAccount);
            return View(transfers.ToList());
        }

        // GET: Transfers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transfer transfer = db.Transfers.Find(id);
            if (transfer == null)
            {
                return HttpNotFound();
            }
            return View(transfer);
        }

        // GET: Transfers/Create
        public ActionResult Create()
        {
            ViewBag.destinationAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name");
            ViewBag.sourceAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name");
            return View();
        }

        // POST: Transfers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "transferID,sourceAccountID,destinationAccountID,date,ammount,exchangeRate")] Transfer transfer)
        {
            FinancialAccount sourceAccount = db.FinancialAccounts.Find(transfer.sourceAccountID);
            FinancialAccount destinationAccount = db.FinancialAccounts.Find(transfer.destinationAccountID);
            if (ModelState.IsValid)
            {
                if (sourceAccount.accountBalance >= transfer.ammount)
                {
                    if (WithdrawFromSource(sourceAccount, transfer.ammount))
                    {
                        if (!DepositToTarget(destinationAccount, transfer.ammount*transfer.exchangeRate))
                        {
                            Console.WriteLine($"Failed to transfer money to {transfer.destinationAccount.name}!");
                            Console.WriteLine($"Attempting to return money to {transfer.sourceAccount.name}...");
                            if (!DepositToTarget(transfer.sourceAccount, transfer.ammount))
                            {
                                Console.WriteLine($"Failed to return money to {transfer.sourceAccount.name}!");
                                Console.WriteLine($"Please re-add the money mannually.");
                            }
                            else
                            {
                                Console.WriteLine($"Returned the money to {transfer.sourceAccount.name}!");
                            }
                        }
                        db.Transfers.Add(transfer);
                        db.SaveChanges();
                        Console.WriteLine($"Transfer Succesful!");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get money from {transfer.sourceAccount.name}!");
                        Console.WriteLine($"Aborting...");
                    }
                }
                else
                {
                    Console.WriteLine($"Not Enough funds! Please add at least {transfer.ammount - transfer.sourceAccount.accountBalance} " +
                        $"to {transfer.sourceAccount.name} and try again!");
                }

                return RedirectToAction("Index");
            }

            ViewBag.destinationAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.destinationAccountID);
            ViewBag.sourceAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.sourceAccountID);
            return View(transfer);
        }
        public bool WithdrawFromSource([Bind(Include = "financialAccountID,name,accountBalance,currency")] FinancialAccount sourceAccount, decimal ammount)
        {
            if (ModelState.IsValid)
            {
                sourceAccount.accountBalance = sourceAccount.accountBalance - ammount;
                db.Entry(sourceAccount).State = EntityState.Modified;
                db.SaveChanges();
                return (true);              
            }
            return (false);
        }
        public bool DepositToTarget([Bind(Include = "financialAccountID,name,accountBalance,currency")] FinancialAccount targetAccount, decimal ammount)
        {
            if (ModelState.IsValid)
            {
                targetAccount.accountBalance = targetAccount.accountBalance + ammount;

                db.Entry(targetAccount).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        // GET: Transfers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transfer transfer = db.Transfers.Find(id);
            if (transfer == null)
            {
                return HttpNotFound();
            }
            ViewBag.destinationAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.destinationAccountID);
            ViewBag.sourceAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.sourceAccountID);
            return View(transfer);
        }

        // POST: Transfers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "transferID,sourceAccountID,destinationAccountID,date,ammount,exchangeRate")] Transfer transfer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transfer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.destinationAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.destinationAccountID);
            ViewBag.sourceAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", transfer.sourceAccountID);
            return View(transfer);
        }

        // GET: Transfers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transfer transfer = db.Transfers.Find(id);
            if (transfer == null)
            {
                return HttpNotFound();
            }
            return View(transfer);
        }

        // POST: Transfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transfer transfer = db.Transfers.Find(id);
            FinancialAccount sourceAccount = transfer.destinationAccount;
            FinancialAccount destinationAccount = transfer.sourceAccount;
            if (transfer == null)
            {
                return HttpNotFound();
            }
            if (sourceAccount.accountBalance >= transfer.ammount)
            {
                if (WithdrawFromSource(sourceAccount, transfer.ammount * transfer.exchangeRate ))
                {
                    if (!DepositToTarget(destinationAccount, transfer.ammount))
                    {
                        Console.WriteLine($"Failed to transfer money to {destinationAccount.name}!");
                        Console.WriteLine($"Attempting to return money to {sourceAccount.name}...");
                        if (!DepositToTarget(sourceAccount, transfer.ammount))
                        {
                            Console.WriteLine($"Failed to return money to {sourceAccount.name}!");
                            Console.WriteLine($"Please re-add the money mannually.");
                        }
                        else
                        {
                            Console.WriteLine($"Returned the money to {sourceAccount.name}!");
                        }
                    }
                    
                    Console.WriteLine($"Transfer Succesfully reverted!");
                }
                else
                {
                    Console.WriteLine($"Failed to get money from {sourceAccount.name}!");
                    Console.WriteLine($"Aborting...");
                }
            }
            else
            {
                Console.WriteLine($"Not Enough funds! Please add at least {transfer.ammount - sourceAccount.accountBalance} " +
                    $"to {sourceAccount.name} and try again!");
            }
            db.Transfers.Remove(transfer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
