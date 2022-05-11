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
    public enum OperationType { Add = 0, Edit = 1, Remove = 2 }
    [Authorize]
    public class ExpenseRecordsController : Controller
    {
        private MyFinancesContext db = new MyFinancesContext();

        // GET: ExpenseRecords
        public ActionResult Index()
        {
            var expenseRecords = db.ExpenseRecords.Include(r => r.financialAccount).Include(r => r.category);
            return View(expenseRecords.ToList());
        }

        // GET: ExpenseRecords/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseRecords expenseRecord = db.ExpenseRecords.Find(id);
            if (expenseRecord == null)
            {
                return HttpNotFound();
            }
            return View(expenseRecord);
        }

        // GET: ExpenseRecords/Create
        public ActionResult Create()
        {
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name");
            PopulateCategoryDropdownList();
            return View();
        }

        // POST: ExpenseRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "expenseRecordID,categoryID,financialAccountID,ammount,details,date")] ExpenseRecords expenseRecord)
        {
            if (ModelState.IsValid)
            {
                db.ExpenseRecords.Add(expenseRecord);
                db.SaveChanges();
                FinancialAccount destinationAccount = db.FinancialAccounts.Find(expenseRecord.financialAccountID);
                if (destinationAccount == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateAccountTotal(destinationAccount, expenseRecord, OperationType.Add);
                Category destinationCategory = db.Categories.Find(expenseRecord.categoryID);
                if (destinationCategory == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateCategoryTotal(destinationCategory, expenseRecord, OperationType.Add);
                return RedirectToAction("Index");
            }

            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", expenseRecord.financialAccountID);
            PopulateCategoryDropdownList(expenseRecord.categoryID);
            return View(expenseRecord);
        }

        // GET: expenseRecords/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseRecords expenseRecord = db.ExpenseRecords.Find(id);
            if (expenseRecord == null)
            {
                return HttpNotFound();
            }
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", expenseRecord.financialAccountID);
            PopulateCategoryDropdownList(expenseRecord.categoryID);
            return View(expenseRecord);
        }

        // POST: ExpenseRecords/Edit/5 
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "expenseRecordID,categoryID,financialAccountID,ammount,details,date")] ExpenseRecords expenseRecord)
        {
            if (ModelState.IsValid)
            {
                FinancialAccount targetAccount = db.FinancialAccounts.Find(expenseRecord.financialAccountID);
                if (targetAccount == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateAccountTotal(targetAccount, expenseRecord, OperationType.Edit);
                Category destinationCategory = db.Categories.Find(expenseRecord.categoryID);
                if (destinationCategory == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UpdateCategoryTotal(destinationCategory, expenseRecord, OperationType.Edit);
                db.Entry(expenseRecord).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.financialAccountID = new SelectList(db.FinancialAccounts, "financialAccountID", "name", expenseRecord.financialAccountID);
            PopulateCategoryDropdownList(expenseRecord.categoryID);
            return View(expenseRecord);
        }
        public void UpdateAccountTotal([Bind(Include = "financialAccountID,name,accountBalance,currency")] FinancialAccount targetAccount, ExpenseRecords expenseRecord, OperationType operation)
        {
            if (ModelState.IsValid)
            { 
                if (operation == OperationType.Add)
                {
                    targetAccount.accountBalance = targetAccount.accountBalance - expenseRecord.ammount;
                }
                else if (operation == OperationType.Remove)
                {
                    targetAccount.accountBalance = targetAccount.accountBalance + expenseRecord.ammount;
                }
                else
                {
                    IEnumerable<ExpenseRecords> recordQuery = from recr in db.ExpenseRecords.AsNoTracking().Where(recr => recr.expenseRecordID == expenseRecord.expenseRecordID) select recr;
                    decimal oldAmmount = recordQuery.ElementAt(0).ammount;

                    targetAccount.accountBalance = targetAccount.accountBalance + oldAmmount - expenseRecord.ammount;
                }
                db.Entry(targetAccount).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        public void UpdateCategoryTotal([Bind(Include = "categoryID,name,total")] Category targetCategory, ExpenseRecords expenseRecord, OperationType operation)
        {
            if (ModelState.IsValid)
            {
                if (operation == OperationType.Add)
                {
                    targetCategory.total = targetCategory.total + expenseRecord.ammount;
                }
                else if (operation == OperationType.Remove)
                {
                    targetCategory.total = targetCategory.total - expenseRecord.ammount;
                }
                else
                {
                    IEnumerable<ExpenseRecords> recordQuery = from recr in db.ExpenseRecords.AsNoTracking().Where(recr => recr.expenseRecordID == expenseRecord.expenseRecordID) select recr;
                    decimal oldAmmount = recordQuery.ElementAt(0).ammount;

                    targetCategory.total = targetCategory.total - oldAmmount + expenseRecord.ammount;
                }
            
                db.Entry(targetCategory).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        // GET: ExpenseRecords/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseRecords expenseRecord = db.ExpenseRecords.Find(id);
            if (expenseRecord == null)
            {
                return HttpNotFound();
            }
            return View(expenseRecord);
        }

        // POST: ExpenseRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ExpenseRecords expenseRecord = db.ExpenseRecords.Find(id);
            Console.WriteLine($"The expenseRecord ID is {expenseRecord.expenseRecordID}");
            FinancialAccount sourceAccount = db.FinancialAccounts.Find(expenseRecord.financialAccountID);
            Console.WriteLine($"The account ID is {expenseRecord.expenseRecordID}");
            if (sourceAccount == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UpdateAccountTotal(sourceAccount, expenseRecord, OperationType.Remove);
            Category destinationCategory = db.Categories.Find(expenseRecord.categoryID);
            if (destinationCategory == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UpdateCategoryTotal(destinationCategory, expenseRecord, OperationType.Remove);
            db.ExpenseRecords.Remove(expenseRecord);
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
            var categoryQuery = from cat in db.Categories.Where(cat => cat.categoryType == CategoryType.Expense) select cat;
            ViewBag.categoryID = new SelectList(categoryQuery, "categoryID", "name");
        }
        private void PopulateCategoryDropdownList(object selectedCategory)
        {
            var categoryQuery = from cat in db.Categories.Where(cat => cat.categoryType == CategoryType.Expense) select cat;
            ViewBag.categoryID = new SelectList(categoryQuery, "categoryID", "name", selectedCategory);
        }
    }
}
