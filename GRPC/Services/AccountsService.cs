﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Librairies.Data;
using Librairies.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRPC
{
    public class AccountsService : ProtoAccount.ProtoAccountBase
    {
        private readonly ProjectCContext _context;
        public AccountsService(ProjectCContext context)
        {
            _context = context;
        }
        public override async Task<ProtoAccountModel> AccountCreate(ProtoAccountModel request, ServerCallContext context)
        {
            _context.Account.Add(ProtoAccountModelToAccount(request));
            await _context.SaveChangesAsync();

            return await Task.FromResult(request);
        }
        public override async Task<ProtoAccountResponse> AccountList(Empty request, ServerCallContext context)
        {
            List<Account> accounts = await _context.Account.ToListAsync();
            ProtoAccountResponse response = new ProtoAccountResponse();
            foreach (var acc in accounts)
            {
                response.Account.Add(AccountToProtoCompteModel(acc));
            }
            return await Task.FromResult(response);
        }
        public override async Task<Empty> AccountDeleteAll(Empty request, ServerCallContext context)
        {
            _context.Account.RemoveRange(_context.Account);
            await _context.SaveChangesAsync();
            return new Empty();
        }

        public override async Task<ProtoAccountModel> AccountFind(ProtoAccountNumber request, ServerCallContext context)
        {
            var account = await _context.Account.FindAsync(request.AccountNumber);

            //TODO CHECK IF NULL
            /*if (account == null)
            {
                return null;
            }*/

            return await Task.FromResult(AccountToProtoCompteModel(account));
        }
        public override async Task<ProtoAccountModel> AccountUpdate(ProtoAccountModel request, ServerCallContext context)
        {
            //TODO CHECK IF NULL or NOT FOUND
            /*if (id != account.AccountNumber)
            {
                return BadRequest();
            }*/

            _context.Entry(ProtoAccountModelToAccount(request)).State = EntityState.Modified;

            //try
            //{
            await _context.SaveChangesAsync();
            /*}
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();*/
            return await Task.FromResult(request);
        }
        public override async Task<Empty> AccountDelete(ProtoAccountNumber request, ServerCallContext context)
        {
            var account = await _context.Account.FindAsync(request.AccountNumber);
            //TODO CHECK IF NULL
            /*if (account == null)
            {
                return NotFound();
            }*/

            _context.Account.Remove(account);
            await _context.SaveChangesAsync();

            return new Empty();
        }
        public override async Task<ProtoTransactionResponse> GetTransactionsByAccount(ProtoAccountNumber request, ServerCallContext context)
        {
            List<Transaction> transactions = await _context.Transaction.Where(s => (s.TransactionOrigin.Equals(request.AccountNumber) || s.TransactionDestination.Equals(request.AccountNumber))).ToListAsync();

            ProtoTransactionResponse response = new ProtoTransactionResponse();
            foreach (var trans in transactions)
            {
                response.Transaction.Add(TransactionToProtoTransactionModel(trans));
            }
            return await Task.FromResult(response);
        }
        public override async Task<Empty> DeleteTransactionsByAccount(ProtoAccountNumber request, ServerCallContext context)
        {
            _context.Transaction.RemoveRange(_context.Transaction.Where(s => (s.TransactionOrigin.Equals(request.AccountNumber) || s.TransactionDestination.Equals(request.AccountNumber))));
            await _context.SaveChangesAsync();
            return new Empty();
        }

        public ProtoAccountModel AccountToProtoCompteModel(Account account)
        {
            return new ProtoAccountModel
            {
                AccountNumber = account.AccountNumber,
                AccountBalance = account.AccountBalance,
                //TODO CHECK UTC
                //AccountCreationDate = Timestamp.FromDateTime(DateTime.SpecifyKind(account.AccountCreationDate, DateTimeKind.Utc)),
                AccountCreationDate = Timestamp.FromDateTime(account.AccountCreationDate),
                AccountHolderFirstName = account.AccountHolderFirstName,
                AccountHolderLastName = account.AccountHolderLastName,
                IsActive = account.IsActive
            };
        }
        public Account ProtoAccountModelToAccount(ProtoAccountModel model)
        {
            return new Account
            {
                AccountNumber = model.AccountNumber,
                AccountBalance = model.AccountBalance,
                AccountCreationDate = model.AccountCreationDate.ToDateTime(),
                AccountHolderFirstName = model.AccountHolderFirstName,
                AccountHolderLastName = model.AccountHolderLastName,
                IsActive = model.IsActive
            };
        }
        public ProtoTransactionModel TransactionToProtoTransactionModel(Transaction transaction)
        {
            return new ProtoTransactionModel
            {
                TransactionNumber = transaction.TransactionNumber,
                TransactionAmount = transaction.TransactionAmount,
                //TODO CHECK UTC
                //AccountCreationDate = Timestamp.FromDateTime(DateTime.SpecifyKind(account.AccountCreationDate, DateTimeKind.Utc)),
                TransactionDate = Timestamp.FromDateTime(transaction.TransactionDate),
                TransactionOrigin = transaction.TransactionOrigin,
                TransactionDestination = transaction.TransactionDestination,
                IsValid = transaction.IsValid
            };
        } //TODO REDONDANT
    }
}
