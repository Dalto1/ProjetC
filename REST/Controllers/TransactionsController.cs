﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST.Data;
using REST.Models;

namespace REST.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly RESTContext _context;
        public TransactionsController(RESTContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> TransactionCreate(Transaction transaction)
        {
            _context.Transaction.Add(transaction);
            await _context.SaveChangesAsync(); 
            
            Account compteOrigine = await _context.Account.FindAsync(transaction.TransactionOrigin);
            Account compteDestination = await _context.Account.FindAsync(transaction.TransactionDestination);
            if (transaction.TransactionAmount < 0)
            {
                transaction.isValid = false;
            }
            else
            {
                if (compteOrigine != null) compteOrigine.AccountBalance -= transaction.TransactionAmount;
                if (compteDestination != null) compteDestination.AccountBalance += transaction.TransactionAmount;
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("TransactionFind", new { id = transaction.TransactionNumber }, transaction);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> TransactionList()
        {
            return await _context.Transaction.ToListAsync();
        }
        [HttpDelete]
        public async Task<IActionResult> TransactionDeleteAll()
        {
            _context.Transaction.RemoveRange(_context.Transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> TransactionFind(int id)
        {
            var transaction = await _context.Transaction.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> TransactionUpdate(int id, Transaction transaction)
        {
            //TODO MISE À JOUR DES SOLDES
            if (id != transaction.TransactionNumber)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transaction.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        [HttpPost("{id}")]
        public BadRequestResult Error()
        {
            return BadRequest();
        }
        private bool TransactionExists(int id)
        {
            return _context.Transaction.Any(e => e.TransactionNumber == id);
        }
    }
}