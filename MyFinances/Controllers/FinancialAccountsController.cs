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
    public class FinancialAccountsController : Controller
    {

        private MyFinancesContext db = new MyFinancesContext();

        // GET: FinancialAccounts
        public ActionResult Index()
        {
            return View(db.FinancialAccounts.ToList());
        }

        // GET: FinancialAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinancialAccount financialAccount = db.FinancialAccounts.Find(id);
            var inTransf = from rec in db.Transfers.AsNoTracking().Where(rec => rec.sourceAccountID == financialAccount.financialAccountID) select rec;
            foreach (var inRecord in inTransf)
            {
                financialAccount.transferSource.Add(inRecord);
            }
            var outTransf = from rec in db.Transfers.AsNoTracking().Where(rec => rec.destinationAccountID == financialAccount.financialAccountID) select rec;
            foreach (var outRecord in outTransf)
            {
                financialAccount.transferDestination.Add(outRecord);
            }
            if (financialAccount == null)
            {
                return HttpNotFound();
            }
            return View(financialAccount);
        }

        // GET: FinancialAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FinancialAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "financialAccountID,name,financialAccountBalance,currency,UserId")] FinancialAccount financialAccount)
        {
            if (ModelState.IsValid)
            {
                db.FinancialAccounts.Add(financialAccount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(financialAccount);
        }

        // GET: FinancialAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinancialAccount financialAccount = db.FinancialAccounts.Find(id);
            if (financialAccount == null)
            {
                return HttpNotFound();
            }
            return View(financialAccount);
        }

        // POST: FinancialAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "financialAccountID,name,accountBalance,currency")] FinancialAccount financialAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(financialAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(financialAccount);
        }

        // GET: FinancialAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinancialAccount financialAccount = db.FinancialAccounts.Find(id);
            if (financialAccount == null)
            {
                return HttpNotFound();
            }
            return View(financialAccount);
        }

        // POST: FinancialAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FinancialAccount financialAccount = db.FinancialAccounts.Find(id);
            db.FinancialAccounts.Remove(financialAccount);
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

        public ActionResult FirstTab()
        {
            return PartialView("_FirstTab");
        }

        public ActionResult SecondTab()
        {
            return PartialView("_SecondTab");
        }
        public ActionResult ThirdTab()
        {
            return PartialView("_ThirdTab");
        }
    }
}
