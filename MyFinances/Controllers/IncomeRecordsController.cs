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
    public class IncomeRecordsController : Controller
    {
        private MyFinancesContext db = new MyFinancesContext();

        // GET: IncomeRecords
        public ActionResult Index()
        {
            var incomeRecords = db.IncomeRecords.Include(r => r.financialAccount).Include(r => r.category);
            return View(incomeRecords.ToList());
        }

        // GET: IncomeRecords/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeRecords incomeRecord = db.IncomeRecords.Find(id);
            if (incomeRecord == null)
            {
                return HttpNotFound();
            }
            return View(incomeRecord);
        }

        // GET: IncomeRecords/Create
        public ActionResult Create()
        {
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name");
            PopulateCategoryDropdownList();
            //ViewBag.categoryID = new SelectList(db.Categories, "categoryID", "name");
            return View();
        }

        // POST: IncomeRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "incomeRecordID,categoryID,financialAccountID,ammount,details,date")] IncomeRecords incomeRecord)
        {
            if (ModelState.IsValid)
            {
                db.IncomeRecords.Add(incomeRecord);
                db.SaveChanges();
                FinancialAccount destinationAccount = db.FinancialAccounts.Find(incomeRecord.financialAccountID);
                if (destinationAccount == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateAccountTotal(destinationAccount, incomeRecord, OperationType.Add);
                Category destinationCategory = db.Categories.Find(incomeRecord.categoryID);
                if (destinationCategory == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateCategoryTotal(destinationCategory, incomeRecord, OperationType.Add);
                return RedirectToAction("Index");
            }

            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", incomeRecord.financialAccountID);
            PopulateCategoryDropdownList(incomeRecord.categoryID);
            return View(incomeRecord);
        }

        // GET: incomeRecords/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeRecords incomeRecord = db.IncomeRecords.Find(id);
            if (incomeRecord == null)
            {
                return HttpNotFound();
            }
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", incomeRecord.financialAccountID);
            PopulateCategoryDropdownList(incomeRecord.categoryID);
            return View(incomeRecord);
        }

        // POST: IncomeRecords/Edit/5 
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "incomeRecordID,categoryID,financialAccountID,ammount,details,date")] IncomeRecords incomeRecord)
        {
            if (ModelState.IsValid)
            {
                FinancialAccount targetAccount = db.FinancialAccounts.Find(incomeRecord.financialAccountID);
                if (targetAccount == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateAccountTotal(targetAccount, incomeRecord, OperationType.Edit);
                Category destinationCategory = db.Categories.Find(incomeRecord.categoryID);
                if (destinationCategory == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateCategoryTotal(destinationCategory, incomeRecord, OperationType.Edit);
                db.Entry(incomeRecord).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", incomeRecord.financialAccountID);
            PopulateCategoryDropdownList(incomeRecord.categoryID);
            return View(incomeRecord);
        }
        public void UpdateAccountTotal([Bind(Include = "financialAccountID,name,accountBalance,currency")] FinancialAccount targetAccount, IncomeRecords incomeRecord, OperationType operation)
        {
            if (operation == OperationType.Add)
            {
                targetAccount.accountBalance = targetAccount.accountBalance + incomeRecord.ammount;
            }
            else if (operation == OperationType.Remove)
            {
                targetAccount.accountBalance = targetAccount.accountBalance - incomeRecord.ammount;
            }
            else
            {
                IEnumerable<IncomeRecords> recordQuery = from recr in db.IncomeRecords.AsNoTracking().Where(recr => recr.incomeRecordID == incomeRecord.incomeRecordID) select recr;
                decimal oldAmmount = recordQuery.ElementAt(0).ammount;
                targetAccount.accountBalance = targetAccount.accountBalance - oldAmmount + incomeRecord.ammount;
            }
            if (ModelState.IsValid)
            {
                db.Entry(targetAccount).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        public void UpdateCategoryTotal([Bind(Include = "categoryID,name,total")] Category targetCategory, IncomeRecords incomeRecord, OperationType operation)
        {
            if (ModelState.IsValid)
            {
                if (operation == OperationType.Add)
                {
                    targetCategory.total = targetCategory.total + incomeRecord.ammount;
                }
                else if (operation == OperationType.Remove)
                {
                    targetCategory.total = targetCategory.total - incomeRecord.ammount;
                }
                else
                {
                    IEnumerable<IncomeRecords> recordQuery = from recr in db.IncomeRecords.AsNoTracking().Where(recr => recr.incomeRecordID == incomeRecord.incomeRecordID) select recr;
                    decimal oldAmmount = recordQuery.ElementAt(0).ammount;

                    targetCategory.total = targetCategory.total - oldAmmount + incomeRecord.ammount;
                }

                db.Entry(targetCategory).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        // GET: IncomeRecords/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IncomeRecords incomeRecord = db.IncomeRecords.Find(id);
            if (incomeRecord == null)
            {
                return HttpNotFound();
            }
            return View(incomeRecord);
        }

        // POST: IncomeRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IncomeRecords incomeRecord = db.IncomeRecords.Find(id);
            Console.WriteLine($"The incomeRecord ID is {incomeRecord.incomeRecordID}");
            FinancialAccount sourceAccount = db.FinancialAccounts.Find(incomeRecord.financialAccountID);
            Console.WriteLine($"The account ID is {incomeRecord.incomeRecordID}");
            if (sourceAccount == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UpdateAccountTotal(sourceAccount, incomeRecord, OperationType.Remove);
            Category destinationCategory = db.Categories.Find(incomeRecord.categoryID);
            if (destinationCategory == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UpdateCategoryTotal(destinationCategory, incomeRecord, OperationType.Remove);
            db.IncomeRecords.Remove(incomeRecord);
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
        private void PopulateCategoryDropdownList()
        {
            var categoryQuery = from cat in db.Categories.Where(cat => cat.categoryType == CategoryType.Income) select cat;
            ViewBag.categoryID = new SelectList(categoryQuery, "categoryID", "name");
        }
        private void PopulateCategoryDropdownList(object selectedCategory)
        {
            var categoryQuery = from cat in db.Categories.Where(cat => cat.categoryType == CategoryType.Income) select cat;
            ViewBag.categoryID = new SelectList(categoryQuery, "categoryID", "name", selectedCategory);
        }
    }
}
