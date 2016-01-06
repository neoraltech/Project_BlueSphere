using System;
using System.Web.Mvc;
using TestExam.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestExam.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private WaveCellSms _smsMessage = new WaveCellSms();
        private string _accountId = ConfigurationManager.AppSettings["AccountId"].ToString();
        private string _subAccountId = ConfigurationManager.AppSettings["SubAccountId"].ToString();
        private string _password = ConfigurationManager.AppSettings["Password"].ToString();
        private readonly string _umId = "0";
        private readonly string _ascii = "ASCII";
        private Wavecell.SendSoapClient sm = new Wavecell.SendSoapClient("SendSoap");

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WaveCell()
        {
            if (TempData["CustomError"] != null)
            {
                ModelState.AddModelError(string.Empty, TempData["CustomError"].ToString());
            }

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult SignalR()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SmsTest(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SmsTest(WaveCellSms model, string returnUrl)
        {
            _smsMessage = model;

            SendStatus result = await Task.Run(() => SendSms());

            switch (result)
            {
                case SendStatus.Success:
                    return RedirectToLocal("/Home/WaveCell");
                case SendStatus.ErrorSource:
                case SendStatus.ErrorDestination:
                case SendStatus.ErrorBody:
                    return View("WaveCell");
                default:
                    return View("Error");
            }
        }

        private SendStatus SendSms()
        {
            if(!CheckSource())
            {
                return SendStatus.ErrorSource;
            }

            if(!CheckDestination())
            {
                return SendStatus.ErrorDestination;
            }

            if(!CheckBody())
            {
                return SendStatus.ErrorBody;
            }

            WaveCellResponse response = SendSmsNow();

            if(response.Success)
            {
                TempData["CustomError"] = "Sms Successfully Sent! Would you like to try again? :)";
                return SendStatus.Success;
            }
            else
            {
                TempData["CustomError"] = "Oops! Sms Sending Failed.";
                return SendStatus.Failed;
            }
        }

        private WaveCellResponse SendSmsNow()
        {
            string scheduledDelivery = CreateScheduledDelivery();
            string subResponse = sm.SendMT(_accountId, _subAccountId, _password, _umId, _smsMessage.Destination, _smsMessage.Source, _smsMessage.Body, _ascii, scheduledDelivery);

            WaveCellResponse response = new WaveCellResponse();

            if(subResponse.Contains("RECEIVED"))
            {
                response.Success = true;
                response.Message = subResponse;
            }
            else
            {
                response.Success = false;
                response.Message = "Oops! It Failed!";
            }

            return response;
        }

        private bool CheckSource()
        {
            Regex regEx = new Regex("^[a-zA-Z][a-zA-Z0-9]*$");

            if(string.IsNullOrEmpty(_smsMessage.Source))
            {
                TempData["CustomError"] = "No input detected on Source.";
                return false;
            }
            else
            {
                if(_smsMessage.Source.Length == 0)
                {
                    return true;
                }
                else if (_smsMessage.Source.Length > 0 &&_smsMessage.Source.Length < 20)
                {
                    if (regEx.Match(_smsMessage.Source).Success)
                    {
                        return true;
                    }
                    else
                    {
                        TempData["CustomError"] = "Only AlphaNumeric values are allowed.";
                        return false;
                    }
                }
                else
                {
                    TempData["CustomError"] = "Please limit your Source to only 20 characters.";
                    return false;
                }
            }
        }

        private bool CheckDestination()
        {
            Regex regEx = new Regex("^[0-9\\+]*$");

            if (string.IsNullOrEmpty(_smsMessage.Destination))
            {
                TempData["CustomError"] = "No input detected on Destination.";
                return false;
            }
            else
            {
                if (regEx.Match(_smsMessage.Destination).Success)
                {
                    _smsMessage.Destination = (_smsMessage.Destination.Contains("+")) ? _smsMessage.Destination.Replace("+", "") : _smsMessage.Destination;
                    return true;
                }
                else
                {
                    TempData["CustomError"] = "Only Numeric values are allowed.";
                    return false;
                }
            }
        }

        private bool CheckBody()
        {
            if (string.IsNullOrEmpty(_smsMessage.Body))
            {
                TempData["CustomError"] = "No input detected on Body.";
                return false;
            }
            else
            {
                if(_smsMessage.Body.Length == 0)
                {
                    return true;
                }
                else if (_smsMessage.Body.Length > 0 && _smsMessage.Body.Length < 500)
                {
                    return true;
                }
                else
                {
                    TempData["CustomError"] = "Only 500 chars are allowed.";
                    return false;
                }
            }
        }

        private string CreateScheduledDelivery()
        {
            if(string.IsNullOrEmpty(_smsMessage.Date) && string.IsNullOrEmpty(_smsMessage.Time))
            {
                return "";
            }
            else
            {
                DateTime dateOnly = Convert.ToDateTime(_smsMessage.Date);
                DateTime timeOnly = Convert.ToDateTime(_smsMessage.Time);
                DateTime convertedDateTime = dateOnly.Add(timeOnly.TimeOfDay);
                DateTime utcDateTime = convertedDateTime.ToUniversalTime();
                return utcDateTime.ToString("s");
            }
        }
    }
}
