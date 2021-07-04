﻿using BrightChain.EntityFrameworkCore.Data;
using BrightChain.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace BrightChain.Tests
{
    [TestClass]
    public class BrightChainEFTest
    {
        [TestMethod]
        public async Task testContextInitializes()
        {
            //create In Memory Database
            var options = new DbContextOptionsBuilder<BrightChainBlockDbContext>()
            .UseBrightChainDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new BrightChainBlockDbContext(options))
            {
            }

            //var brightChainUser = await context.CreateUserAsync();
        }
    }
}