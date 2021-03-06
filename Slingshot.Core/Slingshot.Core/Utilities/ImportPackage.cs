using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Slingshot.Core.Model;
using System.IO.Compression;
using Ionic.Zip;

namespace Slingshot.Core.Utilities
{
    /// <summary>
    /// Static class to write import models to the file system
    /// </summary>
    public static class ImportPackage
    {
        private static string _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string _packageDirectory = _appDirectory + "Package";
        private static string _imageDirectory = _appDirectory + "Images";

        private static Dictionary<string, CsvWriter> csvWriters = new Dictionary<string, CsvWriter>();
        private static Dictionary<string, TextWriter> textWriters = new Dictionary<string, TextWriter>();

        private static List<FamilyAddress> _familyAddresses = new List<FamilyAddress>();

        /// <summary>
        /// Gets the package directory.
        /// </summary>
        /// <value>
        /// The package directory.
        /// </value>
        public static string PackageDirectory
        {
            get
            {
                return _packageDirectory;
            }
        }

        /// <summary>
        /// Gets the image directory.
        /// </summary>
        /// <value>
        /// The image directory.
        /// </value>
        public static string ImageDirectory
        {
            get
            {
                return _imageDirectory;
            }
        }

        /// <summary>
        /// Initializes the <see cref="ImportPackage"/> class.
        /// </summary>
        static ImportPackage()
        {
            InitalizePackageFolder();
        }

        /// <summary>
        /// Initalizes the package folder.
        /// </summary>
        public static void InitalizePackageFolder()
        {
            // CSVs
            // delete existing package directory
            if ( Directory.Exists( _packageDirectory ) )
            {
                Directory.Delete( _packageDirectory, true );
            }

            // create fresh package directory
            Directory.CreateDirectory( _packageDirectory );

            // images
            // delete existing package directory
            if ( Directory.Exists( _imageDirectory ) )
            {
                Directory.Delete( _imageDirectory, true );
            }

            // create fresh package directory
            Directory.CreateDirectory( _imageDirectory );
        }

        /// <summary>
        /// Writes to package.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        public static void WriteToPackage<T>( T model )
        {
            var typeName = model.GetType().Name;

            if ( model is IImportModel )
            {
                var importModel = ( IImportModel ) model;
                // check if a textwriter is needed for this model type
                if ( !textWriters.ContainsKey( typeName ) )
                {
                    if ( !Directory.Exists( _packageDirectory ) )
                    {
                        InitalizePackageFolder();
                    }

                    textWriters.Add( typeName, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{importModel.GetFileName()}" ) );

                    // if model is for person create related writers
                    if ( importModel is Person )
                    {
                        // person attributes
                        var personAttributeValue = new PersonAttributeValue();
                        textWriters.Add( personAttributeValue.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{personAttributeValue.GetFileName()}" ) );

                        // person phones
                        var personPhone = new PersonPhone();
                        textWriters.Add( personPhone.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{personPhone.GetFileName()}" ) );

                        // person addresses
                        var personAddress = new PersonAddress();
                        textWriters.Add( personAddress.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{personAddress.GetFileName()}" ) );

                        // person search key
                        var personSearchKey = new PersonSearchKey();
                        textWriters.Add( personSearchKey.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{personSearchKey.GetFileName()}" ) );
                    }

                    if ( importModel is PersonAttributeValue )
                    {
                        var personAttributeValue = new PersonAttributeValue();
                        textWriters.Add( personAttributeValue.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{personAttributeValue.GetFileName()}" ) );
                    }

                    // if model is for financial batch create related writers
                    if ( importModel is FinancialBatch  )
                    {
                        // financial transactions
                        var financialTransaction = new FinancialTransaction();
                        textWriters.Add( financialTransaction.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{financialTransaction.GetFileName()}" ) );

                        // financial transation details
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        textWriters.Add( financialTransactionDetail.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{financialTransactionDetail.GetFileName()}" ) );
                    }

                    // if model is for financial transaction create related writers
                    if ( importModel is FinancialTransaction )
                    {
                        // financial transation details
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        textWriters.Add( financialTransactionDetail.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{financialTransactionDetail.GetFileName()}" ) );
                    }

                    // if model is for group create related writers
                    if ( importModel is Group )
                    {
                        // group member
                        var groupMember = new GroupMember();
                        textWriters.Add( groupMember.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{groupMember.GetFileName()}" ) );

                        // group attributes
                        var groupAttributeValue = new GroupAttributeValue();
                        textWriters.Add( groupAttributeValue.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{groupAttributeValue.GetFileName()}" ) );

                        // group addresses
                        var groupAddress = new GroupAddress();
                        textWriters.Add( groupAddress.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{groupAddress.GetFileName()}" ) );
                    }

                    // if model is for business create related writers
                    if ( importModel is Business )
                    {
                        // business attributes
                        var businessAttributeValue = new BusinessAttributeValue();
                        textWriters.Add( businessAttributeValue.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{businessAttributeValue.GetFileName()}" ) );

                        // business phones
                        var businessPhone = new BusinessPhone();
                        textWriters.Add( businessPhone.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{businessPhone.GetFileName()}" ) );

                        // business addresses
                        var businessAddress = new BusinessAddress();
                        textWriters.Add( businessAddress.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{businessAddress.GetFileName()}" ) );

                        // business contacts
                        var businessContact = new BusinessContact();
                        textWriters.Add( businessContact.GetType().Name, ( TextWriter ) File.CreateText( $@"{_packageDirectory}\{businessContact.GetFileName()}" ) );
                    }
                }

                var txtWriter = textWriters[typeName];

                // check if a csvwriter is needed for this model type
                if ( !csvWriters.ContainsKey( typeName ) )
                {
                    var newCsvWriter = new CsvWriter( txtWriter );
                    csvWriters.Add( typeName, newCsvWriter );
                    newCsvWriter.WriteHeader<T>();
                    //newCsvWriter.Configuration.QuoteAllFields = true;

                    // if model is for person create related writers
                    if ( importModel is Person )
                    {
                        // person attributes
                        var personAttributeValue = new PersonAttributeValue();
                        var newPersonAttributeValueCsvWriter = new CsvWriter( textWriters[personAttributeValue.GetType().Name] );
                        csvWriters.Add( personAttributeValue.GetType().Name, newPersonAttributeValueCsvWriter );
                        newPersonAttributeValueCsvWriter.WriteHeader<PersonAttributeValue>();

                        // person phones
                        var personPhone = new PersonPhone();
                        var newPersonPhoneCsvWriter = new CsvWriter( textWriters[personPhone.GetType().Name] );
                        csvWriters.Add( personPhone.GetType().Name, newPersonPhoneCsvWriter );
                        newPersonPhoneCsvWriter.WriteHeader<PersonPhone>();

                        // person addresses
                        var personAddress = new PersonAddress();
                        var newPersonAddressCsvWriter = new CsvWriter( textWriters[personAddress.GetType().Name] );
                        csvWriters.Add( personAddress.GetType().Name, newPersonAddressCsvWriter );
                        newPersonAddressCsvWriter.WriteHeader<PersonAddress>();

                        // person search keys
                        var personSearchKey = new PersonSearchKey();
                        var newPersonSearchKeyCsvWriter = new CsvWriter( textWriters[personSearchKey.GetType().Name] );
                        csvWriters.Add( personSearchKey.GetType().Name, newPersonSearchKeyCsvWriter );
                        newPersonSearchKeyCsvWriter.WriteHeader<PersonSearchKey>();
                    }

                    if ( importModel is PersonAttributeValue )
                    {
                        var personAttributeValue = new PersonAttributeValue();
                        var newPersonAttributeValueCsvWriter = new CsvWriter( textWriters[personAttributeValue.GetType().Name] );
                        csvWriters.Add( personAttributeValue.GetType().Name, newPersonAttributeValueCsvWriter );
                        newPersonAttributeValueCsvWriter.WriteHeader<PersonAttributeValue>();
                    }

                    // if model is for financial batch create related writers
                    if ( importModel is FinancialBatch )
                    {
                        // financial transaction
                        var financialTransaction = new FinancialTransaction();
                        var newFinancialTransactionCsvWriter = new CsvWriter( textWriters[financialTransaction.GetType().Name] );
                        csvWriters.Add( financialTransaction.GetType().Name, newFinancialTransactionCsvWriter );
                        newFinancialTransactionCsvWriter.WriteHeader<FinancialTransaction>();

                        // financial transaction detail
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        var newFinancialTransactionDetailCsvWriter = new CsvWriter( textWriters[financialTransactionDetail.GetType().Name] );
                        csvWriters.Add( financialTransactionDetail.GetType().Name, newFinancialTransactionDetailCsvWriter );
                        newFinancialTransactionDetailCsvWriter.WriteHeader<FinancialTransactionDetail>();
                    }

                    //if model is for financial transaction, create related writers
                    if ( importModel is FinancialTransaction )
                    {
                        // financial transaction detail
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        var newFinancialTransactionDetailCsvWriter = new CsvWriter( textWriters[financialTransactionDetail.GetType().Name] );
                        csvWriters.Add( financialTransactionDetail.GetType().Name, newFinancialTransactionDetailCsvWriter );
                        newFinancialTransactionDetailCsvWriter.WriteHeader<FinancialTransactionDetail>();
                    }

                    // if model is for group create related writers
                    if ( importModel is Group )
                    {
                        // group member
                        var groupMember = new GroupMember();
                        var newGroupMemberCsvWriter = new CsvWriter( textWriters[groupMember.GetType().Name] );
                        csvWriters.Add( groupMember.GetType().Name, newGroupMemberCsvWriter );
                        newGroupMemberCsvWriter.WriteHeader<GroupMember>();

                        // group attributes
                        var groupAttributeValue = new GroupAttributeValue();
                        var newGroupAttributeValueCsvWriter = new CsvWriter( textWriters[groupAttributeValue.GetType().Name] );
                        csvWriters.Add( groupAttributeValue.GetType().Name, newGroupAttributeValueCsvWriter );
                        newGroupAttributeValueCsvWriter.WriteHeader<GroupAttributeValue>();

                        // group addresses
                        var groupAddress = new GroupAddress();
                        var newGroupAddressCsvWriter = new CsvWriter( textWriters[groupAddress.GetType().Name] );
                        csvWriters.Add( groupAddress.GetType().Name, newGroupAddressCsvWriter );
                        newGroupAddressCsvWriter.WriteHeader<GroupAddress>();
                    }

                    // if model is for business create related writers
                    if ( importModel is Business )
                    {
                        // business attributes
                        var businessAttributeValue = new BusinessAttributeValue();
                        var newBusinessAttributeValueCsvWriter = new CsvWriter( textWriters[businessAttributeValue.GetType().Name] );
                        csvWriters.Add( businessAttributeValue.GetType().Name, newBusinessAttributeValueCsvWriter );
                        newBusinessAttributeValueCsvWriter.WriteHeader<BusinessAttributeValue>();

                        // business phones
                        var businessPhone = new BusinessPhone();
                        var newBusinessPhoneCsvWriter = new CsvWriter( textWriters[businessPhone.GetType().Name] );
                        csvWriters.Add( businessPhone.GetType().Name, newBusinessPhoneCsvWriter );
                        newBusinessPhoneCsvWriter.WriteHeader<BusinessPhone>();

                        // business addresses
                        var businessAddress = new BusinessAddress();
                        var newBusinessAddressCsvWriter = new CsvWriter( textWriters[businessAddress.GetType().Name] );
                        csvWriters.Add( businessAddress.GetType().Name, newBusinessAddressCsvWriter );
                        newBusinessAddressCsvWriter.WriteHeader<BusinessAddress>();

                        // business Contacts
                        var businessContacts = new BusinessContact();
                        var newBusinessContactsCsvWriter = new CsvWriter( textWriters[businessContacts.GetType().Name] );
                        csvWriters.Add( businessContacts.GetType().Name, newBusinessContactsCsvWriter );
                        newBusinessContactsCsvWriter.WriteHeader<BusinessContact>();
                    }
                }

                var csvWriter = csvWriters[typeName];

                csvWriter.WriteRecord<T>( model );

                // if person model write out any related models
                if ( importModel is Person )
                {
                    // person attributes
                    var personAttributeValue = new PersonAttributeValue();
                    var csvPersonAttributeValueWriter = csvWriters[personAttributeValue.GetType().Name];

                    if ( csvPersonAttributeValueWriter != null )
                    {
                        foreach ( var attribute in ( ( Person ) importModel ).Attributes )
                        {
                            csvPersonAttributeValueWriter.WriteRecord( attribute );
                        }
                    }

                    // person phones
                    var personPhone = new PersonPhone();
                    var csvPersonPhoneWriter = csvWriters[personPhone.GetType().Name];

                    if ( csvPersonPhoneWriter != null )
                    {
                        foreach ( var phone in ( ( Person ) importModel ).PhoneNumbers )
                        {
                            csvPersonPhoneWriter.WriteRecord( phone );
                        }
                    }

                    // person addresses
                    var personAddress = new PersonAddress();
                    var csvPersonAddressWriter = csvWriters[personAddress.GetType().Name];

                    if ( csvPersonAddressWriter != null )
                    {
                        foreach ( var address in ( ( Person ) importModel ).Addresses )
                        {
                            if ( ( ( Person ) importModel ).FamilyId.HasValue )
                            {
                                var familyAddress = new FamilyAddress
                                {
                                    FamilyId = ( ( Person ) importModel ).FamilyId.Value,
                                    Street1 = address.Street1,
                                    PostalCode = address.PostalCode.Left( 5 )
                                };

                                var index = _familyAddresses.FindIndex( a => 
                                    a.FamilyId == ( ( Person ) importModel ).FamilyId.Value && 
                                    a.Street1.Equals( address.Street1, StringComparison.OrdinalIgnoreCase ) && 
                                    a.PostalCode.Equals( address.PostalCode.Left( 5 ) ) );

                                if ( index == -1 )
                                {
                                    _familyAddresses.Add( familyAddress );
                                    csvPersonAddressWriter.WriteRecord( address );
                                }
                            }
                            else
                            {
                                csvPersonAddressWriter.WriteRecord( address );
                            }
                        }
                    }

                    // person search keys
                    var personSearchKey = new PersonSearchKey();
                    var csvPersonSearchKeyWriter = csvWriters[personSearchKey.GetType().Name];

                    if ( csvPersonSearchKeyWriter != null )
                    {
                        foreach ( var searchKey in ( ( Person ) importModel ).PersonSearchKeys )
                        {
                            csvPersonSearchKeyWriter.WriteRecord( searchKey );
                        }
                    }
                }

                // if financial model write out any related models
                if ( importModel is FinancialBatch )
                {
                    // write out financial transactions and transaction details
                    var financialTransaction = new FinancialTransaction();
                    var csvFinancialTransactionWriter = csvWriters[financialTransaction.GetType().Name];

                    var financialTransactionDetail = new FinancialTransactionDetail();
                    var csvFinancialTransactionDetailWriter = csvWriters[financialTransactionDetail.GetType().Name];

                    if ( csvFinancialTransactionWriter != null && csvFinancialTransactionDetailWriter != null )
                    {
                        foreach ( var transaction in ( ( FinancialBatch ) importModel ).FinancialTransactions )
                        {
                            csvFinancialTransactionWriter.WriteRecord( transaction );

                            foreach ( var transactionDetail in transaction.FinancialTransactionDetails )
                            {
                                csvFinancialTransactionDetailWriter.WriteRecord( transactionDetail );
                            }
                        }
                    }
                }

                // if financial Transaction model write out any related models
                if ( importModel is FinancialTransaction )
                {
                    var financialTransactionDetail = new FinancialTransactionDetail();
                    var csvFinancialTransactionDetailWriter = csvWriters[financialTransactionDetail.GetType().Name];

                    if ( csvFinancialTransactionDetailWriter != null )
                    {
                        foreach ( var transactionDetail in ( ( FinancialTransaction ) importModel ).FinancialTransactionDetails )
                        {
                            csvFinancialTransactionDetailWriter.WriteRecord( transactionDetail );
                        }
                    }
                }

                // if group model write out any related models
                if ( importModel is Group )
                {
                    // group members
                    var groupMember = new GroupMember();
                    var csvGroupMemberWriter = csvWriters[groupMember.GetType().Name];

                    if ( csvGroupMemberWriter != null )
                    {
                        foreach ( var groupMemberItem in ( ( Group ) importModel ).GroupMembers )
                        {
                            csvGroupMemberWriter.WriteRecord( groupMemberItem );
                        }
                    }

                    // group attributes
                    var groupAttributeValue = new GroupAttributeValue();
                    var csvGroupAttributeValueWriter = csvWriters[groupAttributeValue.GetType().Name];

                    if ( csvGroupAttributeValueWriter != null )
                    {
                        foreach ( var attribute in ( ( Group ) importModel ).Attributes )
                        {
                            csvGroupAttributeValueWriter.WriteRecord( attribute );
                        }
                    }

                    // group addresses
                    var groupAddress = new GroupAddress();
                    var csvGroupAddressWriter = csvWriters[groupAddress.GetType().Name];

                    if ( csvGroupAddressWriter != null )
                    {
                        foreach ( var address in ( ( Group ) importModel ).Addresses )
                        {
                            csvGroupAddressWriter.WriteRecord( address );
                        }
                    }
                }

                // if business model write out any related models
                if ( importModel is Business )
                {
                    // business attributes
                    var personBusinessValue = new BusinessAttributeValue();
                    var csvBusinessAttributeValueWriter = csvWriters[personBusinessValue.GetType().Name];

                    if ( csvBusinessAttributeValueWriter != null )
                    {
                        foreach ( var attribute in ( ( Business ) importModel ).Attributes )
                        {
                            csvBusinessAttributeValueWriter.WriteRecord( attribute );
                        }
                    }

                    // business phones
                    var businessPhone = new BusinessPhone();
                    var csvBusinessPhoneWriter = csvWriters[businessPhone.GetType().Name];

                    if ( csvBusinessPhoneWriter != null )
                    {
                        foreach ( var phone in ( ( Business ) importModel ).PhoneNumbers )
                        {
                            csvBusinessPhoneWriter.WriteRecord( phone );
                        }
                    }

                    // business addresses
                    var businessAddress = new BusinessAddress();
                    var csvBusinessAddressWriter = csvWriters[businessAddress.GetType().Name];

                    if ( csvBusinessAddressWriter != null )
                    {
                        foreach ( var address in ( ( Business ) importModel ).Addresses )
                        {
                            csvBusinessAddressWriter.WriteRecord( address );
                        }
                    }

                    // business contacts
                    var businessContact = new BusinessContact();
                    var csvBusinessContactWriter = csvWriters[businessContact.GetType().Name];

                    if ( csvBusinessContactWriter != null )
                    {
                        foreach ( var contact in ( ( Business ) importModel ).Contacts )
                        {
                            csvBusinessAddressWriter.WriteRecord( contact );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalizes the package.
        /// </summary>
        /// <param name="exportFilename">The export filename.</param>
        public static void FinalizePackage( string exportFilename )
        {
            // close all csvWriters
            foreach ( var csvWriter in csvWriters )
            {
                csvWriter.Value.Dispose();
            }

            // close all textwriters
            foreach ( var textWriter in textWriters )
            {
                textWriter.Value.Close();
                textWriter.Value.Dispose();
            }

            csvWriters.Clear();
            textWriters.Clear();

            // zip CSV files
            if ( exportFilename.EndsWith( ".slingshot", StringComparison.OrdinalIgnoreCase ) )
            {
                // remove the .slingshot extenstion if it was specified, so we can get just the filename without it
                exportFilename = exportFilename.Substring( 0, exportFilename.Length - ".slingshot".Length );
            }

            var csvZipFile = _appDirectory + exportFilename + ".slingshot";

            if ( File.Exists( csvZipFile ) )
            {
                File.Delete( csvZipFile );
            }

            using ( ZipFile csvZip = new ZipFile() )
            {
                var csvFiles = Directory.GetFiles( _packageDirectory );

                foreach ( var file in csvFiles )
                {
                    csvZip.AddFile( file, "" );
                }

                csvZip.Save( csvZipFile );
            }

            // zip image files
            var files = Directory.GetFiles( _imageDirectory );
            if ( files.Any() )
            {
                long length = 0;
                int fileCounter = 0;

                ZipFile zip = new ZipFile();

                foreach ( var file in files )
                {
                    // over 100MB
                    if ( length < 104857600 )
                    {
                        zip.AddFile( file, "" );
                    }
                    else
                    {
                        length = 0;
                        zip.Save( _appDirectory + exportFilename + "_" + fileCounter + ".Images.slingshot" );
                        fileCounter++;
                        zip.Dispose();
                        zip = new ZipFile();
                        zip.AddFile( file, "" );
                    }

                    length += new System.IO.FileInfo( file ).Length;
                }

                zip.Save( _appDirectory + exportFilename + "_" + fileCounter + ".Images.slingshot" );
                zip.Dispose();
            }

            // delete package folder
            if ( Directory.Exists( _packageDirectory ) )
            {
                Directory.Delete( _packageDirectory, true );
            }

            // delete images folder
            if ( Directory.Exists( _imageDirectory ) )
            {
                Directory.Delete( _imageDirectory, true );
            }
        }

        private class FamilyAddress
        {
            public int FamilyId { get; set; }
            public string Street1 { get; set; }
            public string PostalCode { get; set; }
        }
    }
}