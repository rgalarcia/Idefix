using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections;

namespace Idefix
{
    public class Funciones
    {
        public Archivo LeerArchivo(string fileName)
        {
            Archivo fichero = new Archivo();

            using (BinaryReader read = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                List<double> msgCAT10 = new List<double>();
                List<double> msgCAT20 = new List<double>();
                List<double> msgCAT21 = new List<double>();
                List<int> CAT1920 = new List<int>();

                int pos = 0;
                int end = (int)read.BaseStream.Length;
                int lengthMsg = 0;
                bool isStarting = true;

                double category = read.BaseStream.ReadByte();
                pos++;

                while (pos <= end)
                {
                    if (isStarting)
                    {
                        byte byte1 = (byte)read.BaseStream.ReadByte();
                        byte byte2 = (byte)read.BaseStream.ReadByte();
                        lengthMsg = (byte1 << 8 | byte2) - 3;
                        isStarting = false;
                        pos += 2;
                    }
                    else
                    {
                        while (lengthMsg != 0)
                        {
                            if (category == 10) msgCAT10.Add(read.BaseStream.ReadByte());
                            else if (category == 19 || category == 20) msgCAT20.Add(read.BaseStream.ReadByte());
                            else if (category == 21) msgCAT21.Add(read.BaseStream.ReadByte());
                            lengthMsg--;
                            pos++;
                        }

                        if (category == 10)
                        {
                            fichero.SetMsgCat10(msgCAT10);
                            msgCAT10.Clear();
                        }
                        else if (category == 19)
                        {
                            CAT1920.Add(1);
                            fichero.SetMsgCat20(msgCAT20, CAT1920);
                            msgCAT20.Clear();
                        }
                        else if (category == 20)
                        {
                            CAT1920.Add(0);
                            fichero.SetMsgCat20(msgCAT20, CAT1920);
                            msgCAT20.Clear();
                        }
                        else if (category == 21)
                        {
                            fichero.SetMsgCat21(msgCAT21);
                            msgCAT21.Clear();
                        }

                        category = read.BaseStream.ReadByte();
                        isStarting = true;
                        pos++;
                    }
                }
            }

            return fichero;
        }

        public List<string[]> GetFSPEC(List<double[]> msgs)
        {
            List<string[]> fspecsList = new List<string[]>();
            if (msgs != null)
            {
                foreach (double[] msg in msgs)
                {
                    double[] fspecInit = new double[4];
                    Array.Copy(msg, fspecInit, 4);
                    List<string> fspecFinal = new List<string>();
                    bool checkbyte = true;
                    int i = 0;

                    while (checkbyte)
                    {
                        string tempFspec = Convert.ToString(Convert.ToInt32(fspecInit[i].ToString(), 10), 2).PadLeft(8, '0');

                        if (tempFspec[tempFspec.Length - 1] == '0')
                        {
                            fspecFinal.Add(tempFspec);
                            checkbyte = false;
                        }
                        else
                        {
                            fspecFinal.Add(tempFspec);
                        }
                        i++;
                    }
                    fspecsList.Add(fspecFinal.ToArray());
                }
            }
            return fspecsList;
        }

        public List<CAT10> ReadCat10(List<double[]> msgcat10_T, List<string[]> FSPEC_T) {
            int a = 0;
            int SAC = 0; int SIC = 0;
            string MsgType = string.Empty;
            string ICAO_Address = string.Empty;
            TimeSpan TimeOfDay = TimeSpan.Zero;
            int TN = 0;
            string[] TRD = Array.Empty<string>(); string[] TS = Array.Empty<string>(); string[] TID = Array.Empty<string>(); string[] FL_T = Array.Empty<string>(); string[] SS = Array.Empty<string>(); string[] Mode3A = Array.Empty<string>();
            double[] PP = new double[2] { 0, 0 }; double[] CP = new double[2] { 0, 0 }; double[] PTV = new double[2] { 0, 0 }; double[] CTV = new double[2] { 0, 0 }; double[] TSO = new double[3] { 0, 0, 0 }; double[] CA = new double[2] { 0, 0 };


            //A VER SI FUNCIONA
            List<CAT10> listCAT10 = new List<CAT10>();
            if (msgcat10_T != null && FSPEC_T != null)
            {
                
                while (a < msgcat10_T.Count)
                {
                    SAC = 0; SIC = 0;
                    MsgType = string.Empty;
                    ICAO_Address = string.Empty;
                    TimeOfDay = TimeSpan.Zero;
                    TN = 0;
                    TRD = Array.Empty<string>(); TS = Array.Empty<string>(); TID = Array.Empty<string>(); FL_T = Array.Empty<string>(); SS = Array.Empty<string>(); Mode3A = Array.Empty<string>();
                    PP = new double[2] { 0, 0 }; CP = new double[2] { 0, 0 }; PTV = new double[2] { 0, 0 }; CTV = new double[2] { 0, 0 }; TSO = new double[3] { 0, 0, 0 }; CA = new double[2] { 0, 0 };

                    string FSPEC_1 = FSPEC_T[a][0];
                    double[] msgcat10 = msgcat10_T[a];
                    // int n = 0;
                    int pos = FSPEC_T[a].Length; // posició de byte en el missatge rebut de categoria 10 SENSE cat,lenght,Fspec.
                    if (FSPEC_1[0] == '1')// FRN = 1: Data Source ID
                    {
                        SAC = Convert.ToInt32(msgcat10[pos]); // assumim que es un vector de double on cada posició és el valor decimal del byte corresponent
                        SIC = Convert.ToInt32(msgcat10[pos + 1]);
                        pos = pos + 2;
                    }// FRN = 1: Data Source ID
                    if (FSPEC_1[1] == '1')// FRN = 2: Message Type
                    {
                        double val = msgcat10[pos];
                        switch (val)
                        {
                            case 1:
                                MsgType = "Target Report";
                                break;
                            case 2:
                                MsgType = "Start of Update Cycle";
                                break;
                            case 3:
                                MsgType = "Periodic Status Message";
                                break;
                            case 4:
                                MsgType = "Event-Triggered Status Message";
                                break;
                        }
                        pos += 1;
                    }// FRN = 2: Message Type
                    if (FSPEC_1[2] == '1')// FRN = 3: Target Report Description
                    {
                        string va = Convert2Binary(msgcat10[pos]);
                        String TYP = String.Empty; string DCR = String.Empty; string CHN = String.Empty; string GBS = String.Empty; string CRT = String.Empty; string SIM = String.Empty; string TST = String.Empty; string RAB = String.Empty; string LOP = String.Empty; string TOT = String.Empty; string SIP = String.Empty;

                        StringBuilder val1 = new StringBuilder(va[0]);
                        val1.Append(va[1]);
                        val1.Append(va[2]);
                        string val = val1.ToString();

                        if (val.Equals("00")) { TYP = "SSR Multilateration"; }
                        else if (val.Equals("01")) { TYP = "Mode S Multilateration"; }
                        else if (val.Equals("10")) { TYP = "ADS-B"; }
                        else if (val.Equals("11")) { TYP = "PSR"; }
                        else if (val.Equals("100")) { TYP = "Magnetic Loop System"; }
                        else if (val.Equals("101")) { TYP = "HF Multilateration"; }
                        else if (val.Equals("110")) { TYP = "Not Defined"; }
                        else if (val.Equals("111")) { TYP = "Other types"; }


                        if (va[3].Equals('0')) { DCR = "No differential correction"; }
                        else { DCR = "Differential correction"; }


                        if (va[4].Equals('0')) { CHN = "Chain 1"; }
                        else { CHN = "Chain 2"; }


                        if (va[5].Equals('0')) { GBS = "Transponder Ground Bit Not Set"; }
                        else { GBS = "Transponder Ground Bit Set"; }


                        if (va[6].Equals('0')) { CRT = "No Corrupted Reply in Multilateration"; }
                        else { CRT = "Corrupted Replies in Multilateration"; }
                        pos += 1;

                        if (va[7].Equals('1'))
                        {
                            string va2 = Convert2Binary(msgcat10[pos]);


                            if (va2[0].Equals('0')) { SIM = "Actual Target Report"; }
                            else { SIM = "Simulated Target Report"; }


                            if (va2[1].Equals('0')) { TST = "Default"; }
                            else { TST = "Test Target"; }

                            if (va2[2].Equals('0')) { RAB = "Report from Target Responder"; }
                            else { RAB = "Report From Field Monitor (fixed transpoder)"; }


                            StringBuilder val2 = new StringBuilder(va2[3]);
                            val2.Append(va2[4]);


                            if (val2.ToString().Equals("0")) { LOP = "Undetermined"; }
                            else if (val2.ToString().Equals("1")) { LOP = "Loop Start"; }
                            else if (val2.ToString().Equals("10")) { LOP = "Loop Finish"; }

                            StringBuilder val3 = new StringBuilder(va2[5]);
                            val3.Append(va2[6]);

                            if (val3.Equals("0")) { TOT = "Undetermined"; }
                            else if (val3.ToString().Equals("01")) { TOT = "Aircraft"; }
                            else if (val3.ToString().Equals("10")) { TOT = "Ground Vehicle"; }
                            else if (val3.ToString().Equals("11")) { TOT = "Helicopter"; }

                            pos += 1;

                            if (va2[7].Equals('1'))
                            {
                                string va3 = Convert2Binary(msgcat10[pos]);
                                if (va3[0].Equals('0')) { SIP = "Absence of SPI"; }
                                else { SIP = "Special Position Identification"; }
                                pos += 1;
                            }
                            else { SIP = "Null"; }

                        }
                        TRD = new string[11] { TYP, DCR, CHN, GBS, CRT, SIM, TST, RAB, LOP, TOT, SIP };
                    }// FRN = 3: Target Report Description
                    if (FSPEC_1[3] == '1') //FRN = 4: Time Of Day
                    {
                        string a1 = Convert2Binary(msgcat10[pos]);
                        string a2 = Convert2Binary(msgcat10[pos + 1]);
                        string a3 = Convert2Binary(msgcat10[pos + 2]);
                        StringBuilder hour = new StringBuilder(a1);
                        hour.Append(a2);
                        hour.Append(a3);
                        string hour_in_seconds = hour.ToString();
                        int Hour = (int)Convert.ToInt64(hour_in_seconds, 2);
                        string prueba = ConvertTime(Hour);
                        Hour /= 128;
                        TimeOfDay = TimeSpan.FromSeconds(Hour); // hh:mm:ss
                        pos += 3;
                    }//FRN = 4: Time Of Day
                    if (FSPEC_1[4] == '1') { pos += 8; } //FRN = 5: we all gon'die
                    if (FSPEC_1[5] == '1') // FRN = 6: Polar Position
                    {
                        string rho1 = Convert2Binary(msgcat10[pos]);
                        string rho2 = Convert2Binary(msgcat10[pos + 1]);
                        StringBuilder rho_BIN = new StringBuilder(rho1);
                        rho_BIN.Append(rho2);
                        string rho_BIN_TOTAL = rho_BIN.ToString();
                        double rho = (int)Convert.ToInt64(rho_BIN_TOTAL, 10); // in m
                        string theta1 = Convert2Binary(msgcat10[pos + 2]);
                        string theta2 = Convert2Binary(msgcat10[pos + 3]);
                        StringBuilder theta_BIN = new StringBuilder(theta1);
                        theta_BIN.Append(theta2);
                        string theta_BIN_TOTAL = '0' + theta_BIN.ToString();
                        int theta_int = Convert.ToInt16(theta_BIN_TOTAL, 2);
                        double theta = theta_int * 360 / 65536; // in degrees
                        PP = new double[2] { rho, theta };
                        pos += 4;
                    } // FRN = 6: Polar Position
                    if (FSPEC_1[6] == '1') // FRN = 7: Cartesian Position
                    {
                        string x1 = Convert2Binary(msgcat10[pos]);
                        string x2 = Convert2Binary(msgcat10[pos + 1]);
                        StringBuilder x_BIN = new StringBuilder(x1);
                        x_BIN.Append(x2);
                        string x_BIN_TOTAL = x_BIN.ToString();
                        double x = (int)Convert.ToInt16(x_BIN_TOTAL, 2); // in m
                        string y1 = Convert2Binary(msgcat10[pos + 2]);
                        string y2 = Convert2Binary(msgcat10[pos + 3]);
                        StringBuilder y_BIN = new StringBuilder(y1);
                        y_BIN.Append(y2);
                        string y_BIN_TOTAL = y_BIN.ToString();
                        double y = ((int)Convert.ToInt16(y_BIN_TOTAL, 2)); // in degrees
                        CP = new double[2] { x, y };
                        pos += 4;
                    }// FRN = 7: Cartesian Position
                    string error = FSPEC_1;
                    char error_in = FSPEC_1[7];
                    if (FSPEC_1[7] == '0') { }
                    else
                    {
                        string FSPEC_2 = FSPEC_T[a][1];
                        if (FSPEC_2[0] == '1') // FRN = 8: Polar Track Velocity
                        {
                            string ground_speed1 = Convert2Binary(msgcat10[pos]);
                            string ground_speed2 = Convert2Binary(msgcat10[pos + 1]);
                            StringBuilder ground_speed_BIN = new StringBuilder(ground_speed1);
                            ground_speed_BIN.Append(ground_speed2);
                            string ground_speed_BIN_TOTAL = ground_speed_BIN.ToString();
                            double ground_speed = ((int)Convert.ToInt16(ground_speed_BIN_TOTAL, 2)) * 0.22; // in m
                            string track_angle1 = Convert2Binary(msgcat10[pos + 2]);
                            string track_angle2 = Convert2Binary(msgcat10[pos + 3]);
                            StringBuilder track_angle_BIN = new StringBuilder(track_angle1);
                            track_angle_BIN.Append(track_angle2);
                            string track_angle_BIN_TOTAL = '0' + track_angle_BIN.ToString();
                            int ta_int = Convert.ToInt16(track_angle_BIN_TOTAL, 2);
                            double track_angle = ta_int * 360 / 65536; // in degrees
                            PTV = new double[2] { ground_speed, track_angle };
                            pos += 4;
                        }// FRN = 8: Polar Track Velocity
                        if (FSPEC_2[1] == '1') // FRN = 9: Cartesian Track Velocity
                        {
                            double Vx, Vy;
                            if(SIC.Equals(7))
                            { 
                                string Vx1 = Convert2Binary(msgcat10[pos]);
                                string Vx2 = Convert2Binary(msgcat10[pos + 1]);
                                StringBuilder Vx_BIN = new StringBuilder(Vx1);
                                Vx_BIN.Append(Vx2);
                                string Vx_BIN_TOTAL = Vx_BIN.ToString();
                                Vx = Convert.ToInt16(Vx_BIN_TOTAL, 2) * 0.25; // in m/s
                                string Vy1 = Convert2Binary(msgcat10[pos + 2]);
                                string Vy2 = Convert2Binary(msgcat10[pos + 3]);
                                StringBuilder Vy_BIN = new StringBuilder(Vy1);
                                Vy_BIN.Append(Vy2);
                                string Vy_BIN_TOTAL = Vy_BIN.ToString();
                                Vy = Convert.ToInt16(Vy_BIN_TOTAL, 2) / 4; // in m/s
                            }
                            else
                            {
                                string Vx1 = Convert2Binary(msgcat10[pos]);
                                string Vx2 = Convert2Binary(msgcat10[pos + 1]);
                                StringBuilder Vx_BIN = new StringBuilder(Vx1);
                                Vx_BIN.Append(Vx2);
                                string Vx_BIN_TOTAL = Vx_BIN.ToString();
                                Vx = (int)Convert.ToInt16(Vx_BIN_TOTAL, 2); // in m/s
                                string Vy1 = Convert2Binary(msgcat10[pos + 2]);
                                string Vy2 = Convert2Binary(msgcat10[pos + 3]);
                                StringBuilder Vy_BIN = new StringBuilder(Vy1);
                                Vy_BIN.Append(Vy2);
                                string Vy_BIN_TOTAL = Vy_BIN.ToString();
                                Vy = ((int)Convert.ToInt16(Vy_BIN_TOTAL, 2)); // in m/s
                            }
                            CTV = new double[2] { Vx, Vy };
                            pos += 4;
                        }// FRN = 9: Cartesian Track Velocity
                        if (FSPEC_2[2] == '1') // FRN = 10: Track Number
                        {
                            string tn_1 = Convert2Binary(msgcat10[pos]);
                            string tn_2 = Convert2Binary(msgcat10[pos + 1]);
                            StringBuilder tn_t = new StringBuilder();
                            tn_t.Append(tn_1[4]);
                            tn_t.Append(tn_1[5]);
                            tn_t.Append(tn_1[6]);
                            tn_t.Append(tn_1[7]);
                            tn_t.Append(tn_2);
                            string tn_tot = tn_t.ToString();
                            TN = (int)Convert.ToInt32(tn_tot, 2);
                            pos += 2;
                        }// FRN = 10: Track Number

                        if (FSPEC_2[3] == '1') // FRN = 11: Track Status
                        {
                            string ts = Convert2Binary(msgcat10[pos]);
                            string CNF = String.Empty; string TRE = String.Empty; string CST = String.Empty; string MAH = String.Empty; string TCC = String.Empty; string STH = String.Empty; string TOM = string.Empty; String DOU = String.Empty; string MRS = string.Empty; string GHO = String.Empty;
                            if (ts[0].Equals('1')) { CNF = "Track initialization phase"; }
                            else { CNF = "Confirmed Track"; }

                            if (ts[1].Equals('1')) { TRE = "Last report of track"; }
                            else { TRE = "Default"; }

                            StringBuilder cst = new StringBuilder(ts[2]);
                            cst.Append(ts[3]);
                            if (cst.ToString().Equals("00")) { CST = "No Extrapolation"; }
                            else if (cst.ToString().Equals("01")) { CST = "Predictable extrapolation due to sensor refresh period"; }
                            else if (cst.ToString().Equals("10")) { CST = "Predictable extrapolation in masked area"; }
                            else if (cst.ToString().Equals("11")) { CST = "Extrapolation due to unpredictable absence of detection"; }

                            if (ts[4].Equals('1')) { MAH = "Horizontal manoeuvre"; }
                            else { MAH = "Default"; }

                            if (ts[5].Equals('1')) { TCC = "Slant range correction and a suitable projection technique are used to track in a 2D.reference plane, tangential to the earth model at the Sensor Site co-ordinates."; }
                            else { TCC = "Tracking performed in 'Sensor Plane', i.e. neither slant range correction nor projection was applied"; }

                            if (ts[6].Equals('1')) { STH = "Smoothed position"; }
                            else { STH = "Measured position"; }
                            pos += 1;

                            if (ts[7].Equals('0')) { }
                            else
                            {
                                string ts_1 = Convert2Binary(msgcat10[pos]);
                                StringBuilder tom = new StringBuilder();
                                tom.Append(ts_1[0]);
                                tom.Append(ts_1[1]);
                                if (tom.ToString().Equals("00")) { TOM = "Unknown type of movement "; }
                                else if (tom.ToString().Equals("01")) { TOM = "Taking-off "; }
                                else if (tom.ToString().Equals("10")) { TOM = "Landing"; }
                                else if (tom.ToString().Equals("11")) { TOM = "Other types of movement"; }

                                StringBuilder dou = new StringBuilder(ts_1[2]);
                                dou.Append(ts_1[3]);
                                dou.Append(ts_1[4]);
                                if (dou.ToString().Equals("00")) { DOU = "No doubt "; }
                                else if (dou.ToString().Equals("001")) { DOU = "Doubtful correlation (undetermined reason)"; }
                                else if (dou.ToString().Equals("010")) { DOU = "Doubtful correlation in clutter"; }
                                else if (dou.ToString().Equals("011")) { DOU = "Loss of accuracy"; }
                                else if (dou.ToString().Equals("100")) { DOU = "Loss of accuracy in clutter"; }
                                else if (dou.ToString().Equals("101")) { DOU = "HF Multilateration"; }
                                else if (dou.ToString().Equals("110")) { DOU = "Unstable track "; }
                                else if (dou.ToString().Equals("111")) { DOU = "Previously coasted"; }

                                StringBuilder mrs = new StringBuilder(ts_1[5]);
                                mrs.Append(ts_1[6]);
                                if (mrs.ToString().Equals("0")) { MRS = "Merge or split indication undetermined"; }
                                else if (mrs.ToString().Equals("1")) { MRS = "Track merged by association to plot"; }
                                else if (mrs.ToString().Equals("10")) { MRS = "Track merged by non-association to plot"; }
                                else if (mrs.ToString().Equals("11")) { MRS = "Split track"; }
                                pos += 1;

                                if (ts_1[7].Equals('0')) { }
                                else
                                {
                                    string ts_2 = Convert2Binary(msgcat10[pos]);
                                    if (ts_2[4].Equals('1')) { GHO = "Default"; }
                                    else { GHO = "Ghost track"; }
                                    pos += 1;
                                }
                            }
                            TS = new string[10] { CNF, TRE, CST, MAH, TCC, STH, TOM, DOU, MRS, GHO };
                        }// FRN = 11; Track Status
                        if (FSPEC_2[4] == '1') // FRN = 12: Mode-3A
                        {
                            string V = "";
                            string G = "";
                            string L = "";
                            string Response = "";

                            string mode3A = Convert2Binary(msgcat10[pos]);
                            string mode3A_2 = Convert2Binary(msgcat10[pos + 1]);
                            if (mode3A[0].Equals('0')) V = "Code validated";
                            else V = "Code not validated";
                            if (mode3A[1].Equals('0')) G = "Default";
                            else G = "Garbled code";
                            if (mode3A[2].Equals('0')) L = "Mode-3/A code derived from the reply of the transponder";
                            else L = "Mode-3/A code not extracted during the last update period";



                            StringBuilder resp = new StringBuilder();
                            resp.Append(mode3A[4]);
                            resp.Append(mode3A[5]);
                            resp.Append(mode3A[6]);
                            resp.Append(mode3A[7]);
                            resp.Append(mode3A_2);

                            string resp1 = resp.ToString();
                            int resp2 = Convert.ToInt16(resp1, 2);

                            Response = Convert.ToString(resp2, 8);

                            Mode3A = new string[4] { V, G, L, Response };
                            pos += 2;
                        }// FRN = 12: Mode-3A
                        if (FSPEC_2[5] == '1') // FRN = 13: ICAO address
                        {
                            string ICAO1 = Convert2Binary(msgcat10[pos]);
                            string ICAO2 = Convert2Binary(msgcat10[pos + 1]);
                            string ICAO3 = Convert2Binary(msgcat10[pos + 2]);
                            StringBuilder ICAO = new StringBuilder(ICAO1);
                            ICAO.Append(ICAO2);
                            ICAO.Append(ICAO3);
                            ICAO_Address = ICAO.ToString();

                            pos += 3;
                        }// FRN = 13: ICAO address
                        if (FSPEC_2[6] == '1') // FRN = 14: Target Identification
                        {
                            string sti1 = Convert2Binary(msgcat10[pos]);
                            StringBuilder sti = new StringBuilder(sti1[0]);
                            sti.Append(sti1[1]);
                            string STI = string.Empty;

                            if (sti.ToString().Equals("0")) { STI = "Callsign or registration downlinked from transponder"; }
                            else if (sti.ToString().Equals("1")) { STI = "Registration not downlinked from transponder"; }
                            else if (sti.ToString().Equals("10")) { STI = "Callsign not downlinked from transponder"; }
                            else if (sti.ToString().Equals("11")) { STI = "Not defined"; }


                            string byte2 = Convert2Binary(msgcat10[pos + 1]);
                            StringBuilder tid1 = new StringBuilder();
                            tid1.Append(byte2[0]);
                            tid1.Append(byte2[1]);
                            tid1.Append(byte2[2]);
                            tid1.Append(byte2[3]);
                            tid1.Append(byte2[4]);
                            tid1.Append(byte2[5]);
                            char TID1 = ConvertToIA5(tid1);

                            string byte3 = Convert2Binary(msgcat10[pos + 2]);
                            StringBuilder tid2 = new StringBuilder();
                            tid1.Append(byte2[6]);
                            tid2.Append(byte2[7]);
                            tid2.Append(byte3[0]);
                            tid2.Append(byte3[1]);
                            tid2.Append(byte3[2]);
                            tid2.Append(byte3[3]);
                            char TID2 = ConvertToIA5(tid2);

                            string byte4 = Convert2Binary(msgcat10[pos + 3]);
                            StringBuilder tid3 = new StringBuilder();
                            tid3.Append(byte3[4]);
                            tid3.Append(byte3[5]);
                            tid3.Append(byte3[6]);
                            tid3.Append(byte3[7]);
                            tid3.Append(byte4[0]);
                            tid3.Append(byte4[1]);
                            char TID3 = ConvertToIA5(tid3);

                            StringBuilder tid4 = new StringBuilder();
                            tid4.Append(byte4[2]);
                            tid4.Append(byte4[3]);
                            tid4.Append(byte4[4]);
                            tid4.Append(byte4[5]);
                            tid4.Append(byte4[6]);
                            tid4.Append(byte4[7]);
                            char TID4 = ConvertToIA5(tid4);

                            string byte5 = Convert2Binary(msgcat10[pos + 4]);
                            StringBuilder tid5 = new StringBuilder();
                            tid5.Append(byte5[0]);
                            tid5.Append(byte5[1]);
                            tid5.Append(byte5[2]);
                            tid5.Append(byte5[3]);
                            tid5.Append(byte5[4]);
                            tid5.Append(byte5[5]);
                            char TID5 = ConvertToIA5(tid5);

                            string byte6 = Convert2Binary(msgcat10[pos + 5]);
                            StringBuilder tid6 = new StringBuilder();
                            tid6.Append(byte5[6]);
                            tid6.Append(byte5[7]);
                            tid6.Append(byte6[0]);
                            tid6.Append(byte6[1]);
                            tid6.Append(byte6[2]);
                            tid6.Append(byte6[3]);
                            char TID6 = ConvertToIA5(tid6);

                            string byte7 = Convert2Binary(msgcat10[pos + 6]);
                            StringBuilder tid7 = new StringBuilder();
                            tid7.Append(byte6[4]);
                            tid7.Append(byte6[5]); 
                            tid7.Append(byte6[6]);
                            tid7.Append(byte6[7]);
                            tid7.Append(byte7[0]);
                            tid7.Append(byte7[1]);
                            char TID7 = ConvertToIA5(tid7);

                            StringBuilder tid8 = new StringBuilder();
                            tid8.Append(byte7[2]);
                            tid8.Append(byte7[3]);
                            tid8.Append(byte7[4]);
                            tid8.Append(byte7[5]);
                            tid8.Append(byte7[6]);
                            tid8.Append(byte7[7]);
                            char TID8 = ConvertToIA5(tid8);

                            string TID_T = string.Concat(TID1, TID2, TID3, TID4, TID5, TID6, TID7, TID8);
                            TID = new string[2] { STI, TID_T };
                            pos += 7;
                        }// FRN = 14: Target Identification
                        if (FSPEC_2[7] == '0') { }
                        else
                        {
                            string FSPEC_3 = FSPEC_T[a][2];
                            if (FSPEC_3[2] == '1') // FRN = 17: Flight Level
                            {

                                string V = "";
                                string G = "";
                                int FL = 0;

                                string fl = Convert2Binary(msgcat10[pos]);
                                string fl_2 = Convert2Binary(msgcat10[pos + 1]);
                                if (fl[0].Equals('0')) V = "Code validated";
                                else V = "Code not validated";
                                if (fl[1].Equals('0')) G = "Default";
                                else G = "Garbled code";


                                StringBuilder resp = new StringBuilder();
                                resp.Append(fl[2]);
                                resp.Append(fl[3]);
                                resp.Append(fl[4]);
                                resp.Append(fl[5]);
                                resp.Append(fl[6]);
                                resp.Append(fl[7]);
                                resp.Append(fl_2);

                                int resp1 = Convert.ToInt32(resp.ToString());

                                string FL1 = Convert2Binary(Convert.ToDouble(resp1));
                                FL = Convert.ToInt32(FL1, 2);
                                FL /= 4;

                                FL_T = new string[3] { V, G, FL.ToString() };
                                pos += 2;

                            }// FRN = 17: Flight Level
                            if (FSPEC_3[4] == '1') //FRN = 19; Target Size and Orientation --> TSO
                            {
                                double LEN; double ORI = 0; double WID = 0;
                                string tso = Convert2Binary(msgcat10[pos]);
                                string len = tso.Remove(tso.Length - 1);
                                int LEN_0 = (int)Convert.ToInt32(len, 10);
                                LEN = Convert.ToDouble(LEN_0);
                                pos += 1;
                                if (tso[7].Equals('1'))
                                {
                                    string tso_1 = Convert2Binary(msgcat10[pos]);
                                    string ori = tso_1.Remove(tso.Length - 1);
                                    int ORI_0 = (int)Convert.ToInt32(ori, 10);
                                    ORI = Convert.ToDouble(ORI_0);
                                    ORI *= 360 / 128;
                                    pos += 1;
                                    if (tso_1[7].Equals('1'))
                                    {
                                        string tso_2 = Convert2Binary(msgcat10[pos]);
                                        string wid = tso_2.Remove(tso.Length - 1);
                                        int WID_0 = (int)Convert.ToInt32(wid, 10);
                                        WID = Convert.ToDouble(WID_0);
                                        pos += 1;
                                    }
                                }

                                TSO = new double[3] { LEN, ORI, WID };
                            }//FRN = 19; Target Size and Orientation
                            if (FSPEC_3[5] == '1') //FRN = 20; SYSTEM STATUS
                            {
                                String NOGO = String.Empty;
                                String OVL = String.Empty;
                                String TSV = String.Empty;
                                String DIV = String.Empty;
                                String TTF = String.Empty;
                                string ss = Convert2Binary(msgcat10[pos]);
                                StringBuilder nogo = new StringBuilder(ss[0]);
                                nogo.Append(ss[1]);
                                string nogo_1 = nogo.ToString();
                                switch (nogo_1)
                                {
                                    case "0":
                                        NOGO = "Operational";
                                        break;
                                    case "1":
                                        NOGO = "Degradated";
                                        break;
                                    case "10":
                                        NOGO = "NOGO";
                                        break;
                                }

                                if (ss[2].Equals('1')) { OVL = "Overload"; }
                                else { OVL = "No Overload"; }

                                if (ss[3].Equals('1')) { TSV = "Time Source Invalid"; }
                                else { TSV = "Time Source Valid"; }

                                if (ss[4].Equals('1')) { DIV = "Diversity Degraded"; }
                                else { DIV = "Normal Operation"; }

                                if (ss[5].Equals('1')) { TTF = "Test Target Failure"; }
                                else { TTF = "Test Target Operative"; }

                                SS = new string[5] { NOGO, OVL, TSV, DIV, TTF };
                                pos += 1;
                            }//SS = 20; SYSTEM STATUS
                            if (FSPEC_3[7] == '0') { }
                            else
                            {
                                string FSPEC_4 = FSPEC_T[a][3];
                                if (FSPEC_4[3] == '1') // FRN = 25; Calculated acceleration
                                {
                                    string ax1 = Convert2Binary(msgcat10[pos]);
                                    string ax11 = ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1[0].ToString() + ax1;
                                    double ax = (int)Convert.ToInt16(ax11, 2);
                                    ax /= 4;//m/s^2
                                    string ay1 = Convert2Binary(msgcat10[pos + 1]);
                                    string ay11 = ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1[0].ToString() + ay1;
                                    double ay = ((int)Convert.ToInt16(ay11, 2));
                                    ay /= 4;//m/s^2
                                    CA = new double[2] { ax, ay };
                                    pos += 2;
                                }// FRN = 25; Calculated acceleration
                            }
                        }
                    }
                    CAT10 obj = new CAT10(SIC, SAC, MsgType, TRD, TimeOfDay, PP, CP, PTV, CTV, TN, TS, Mode3A, ICAO_Address, TID, FL_T, TSO, SS, CA);

                    listCAT10.Add(obj);
                    a += 1;
                }
            }
            return listCAT10;
        }

        public List<CAT20> ReadCat20(List<double[]> msgcat20_T, List<string[]> FSPEC_20T, List<int> CAT1920)
        {
            int a = 0;
            List<CAT20> listCAT20 = new List<CAT20>();

            if (msgcat20_T != null && FSPEC_20T != null && CAT1920 != null)
            {
                while (a < msgcat20_T.Count)
                {
                    int SAC = 0; int SIC = 0; double cartesianH = 0; double geometricH = 0; double SIGMA_GH = 0;
                    string ICAO_Address = string.Empty; string VFI = string.Empty; string MsgType = string.Empty; string CAT = string.Empty;
                    TimeSpan TimeOfDay = TimeSpan.Zero;
                    int TN = 0;
                    string[] TRD = Array.Empty<string>(); string[] TS = Array.Empty<string>(); string[] TID = Array.Empty<string>(); string[] PPM = Array.Empty<string>(); string[] CD = Array.Empty<string>(); string[] Mode3A = Array.Empty<string>(); string[] FL_T = Array.Empty<string>(); string[] ModeC = Array.Empty<string>();
                    double[] PP = Array.Empty<double>(); double[] CP = Array.Empty<double>(); double[] PTV = Array.Empty<double>(); double[] CTV = Array.Empty<double>(); double[] TSO = Array.Empty<double>(); double[] CA = Array.Empty<double>(); double[] DOP = Array.Empty<double>(); double[] SDEV = Array.Empty<double>();

                    string FSPEC_1 = FSPEC_20T[a][0];
                    double[] msgcat20 = msgcat20_T[a];
                    // int n = 0;
                    int pos = FSPEC_20T[a].Length; // posició de byte en el missatge rebut de categoria 20 SENSE cat,lenght,Fspec.
                    if (CAT1920[a].Equals(0))
                    {
                        CAT = "20";
                        if (FSPEC_1[0] == '1')// FRN = 1: Data Source ID
                        {
                            SAC = Convert.ToInt32(msgcat20[pos]);
                            SIC = Convert.ToInt32(msgcat20[pos + 1]);
                            pos = pos + 2;
                        }// FRN = 1: Data Source ID

                        if (FSPEC_1[1] == '1')// FRN = 2: Target Report Description
                        {
                            string va = Convert2Binary(msgcat20[pos]);

                            string SSR = String.Empty;
                            if (va[0].Equals('0')) { SSR = "No Non-Mode S 1090MHz multilateration"; }
                            else { SSR = "Non-Mode S 1090MHz multilateration"; }

                            string MS = String.Empty;
                            if (va[1].Equals('0')) { MS = "No Mode-S 1090 MHz multilateration"; }
                            else { MS = "Mode-S 1090 MHz multilateration"; }

                            string HF = String.Empty;
                            if (va[2].Equals('0')) { HF = "Non"; }
                            else { HF = "HF multilateration"; }

                            string VDL4 = String.Empty;
                            if (va[3].Equals('0')) { VDL4 = "No VDL Mode 4 multilateration"; }
                            else { VDL4 = "VDL Mode 4 multilateration"; }

                            string UAT = String.Empty;
                            if (va[4].Equals('0')) { UAT = "No UAT multilateration"; }
                            else { UAT = "UAT multilateration"; }

                            string DME = String.Empty;
                            if (va[5].Equals('0')) { DME = "No UAT multilateration"; }
                            else { DME = "UAT multilateration"; }

                            string OT = String.Empty;
                            if (va[6].Equals('0')) { OT = "No UAT multilateration"; }
                            else { OT = "UAT multilateration"; }

                            string RAB = String.Empty; string SPI = String.Empty; string CHN = String.Empty; string GBS = String.Empty; string CRT = String.Empty; string SIM = String.Empty; string TST = String.Empty;

                            if (va[7].Equals('0')) { pos += 1; }
                            else
                            {
                                pos += 1;
                                string va2 = Convert2Binary(msgcat20[pos]);

                                if (va2[0].Equals('0')) { RAB = "Report from Target Responder"; }
                                else { RAB = "Report From Field Monitor (fixed transpoder)"; }

                                if (va2[1].Equals('0')) { SPI = "Absence of SPI"; }
                                else { SPI = "Special Position Identification"; }

                                if (va2[2].Equals('0')) { CHN = "Chain 1"; }
                                else { CHN = "Chain 2"; }

                                if (va2[3].Equals('0')) { GBS = "Transponder Ground Bit Not Set"; }
                                else { GBS = "Transponder Ground Bit Set"; }

                                if (va2[4].Equals('0')) { CRT = "No Corrupted replies in multilateration"; }
                                else { CRT = "Corrupted replies in multilateration"; }

                                if (va2[5].Equals('0')) { SIM = "Actual Target Report"; }
                                else { SIM = "Simulated Target Report"; }

                                if (va2[6].Equals('0')) { TST = "Default"; }
                                else { TST = "Test Target"; }

                                pos += 1;
                            }
                            TRD = new string[14] { SSR, MS, HF, VDL4, UAT, DME, OT, RAB, SPI, CHN, GBS, CRT, SIM, TST };

                        }// FRN = 2: Target Report Description

                        if (FSPEC_1[2] == '1') //FRN = 3: Time Of Day
                        {
                            string a1 = Convert2Binary(msgcat20[pos]);
                            string a2 = Convert2Binary(msgcat20[pos + 1]);
                            string a3 = Convert2Binary(msgcat20[pos + 2]);
                            StringBuilder hour = new StringBuilder(a1);
                            hour.Append(a2);
                            hour.Append(a3);
                            string hour_in_seconds = hour.ToString();
                            int Hour = (int)Convert.ToInt64(hour_in_seconds, 2);
                            Hour /= 128;
                            TimeOfDay = TimeSpan.FromSeconds(Hour); // hh:mm:ss
                            pos += 3;
                        }//FRN = 3: Time Of Day

                        if (FSPEC_1[3] == '1') { pos += 8; } //FRN = 4: we all gon'die

                        if (FSPEC_1[4] == '1') // FRN = 5: Cartesian Position
                        {
                            string x1 = Convert2Binary(msgcat20[pos]);
                            string x2 = Convert2Binary(msgcat20[pos + 1]);
                            string x3 = Convert2Binary(msgcat20[pos + 2]);
                            StringBuilder x_BIN = new StringBuilder(x1);
                            x_BIN.Append(x2);
                            x_BIN.Append(x3);
                            string x_BIN_TOTAL = x_BIN.ToString();
                            double x = (int)Convert.ToInt64(x_BIN_TOTAL, 2) * 0.5; // in m
                            string y1 = Convert2Binary(msgcat20[pos + 3]);
                            string y2 = Convert2Binary(msgcat20[pos + 4]);
                            string y3 = Convert2Binary(msgcat20[pos + 5]);
                            StringBuilder y_BIN = new StringBuilder(y1);
                            y_BIN.Append(y2);
                            y_BIN.Append(y3);
                            string y_BIN_TOTAL = y_BIN.ToString();
                            double y = ((int)Convert.ToInt64(y_BIN_TOTAL, 2)) * 0.5; // in m
                            CP = new double[2] { x, y };
                            pos += 6;
                        }// FRN = 5: Cartesian Position

                        if (FSPEC_1[5] == '1') // FRN = 6: Track Number
                        {
                            string tn_1 = Convert2Binary(msgcat20[pos]);
                            string tn_2 = Convert2Binary(msgcat20[pos + 1]);
                            StringBuilder tn_t = new StringBuilder(tn_1[4]);
                            tn_t.Append(tn_1[5]);
                            tn_t.Append(tn_1[6]);
                            tn_t.Append(tn_1[7]);
                            tn_t.Append(tn_2);
                            string tn_tot = tn_t.ToString();
                            TN = (int)Convert.ToInt32(tn_tot, 2);
                            pos += 2;
                        }// FRN = 6: Track Number

                        if (FSPEC_1[6] == '1') // FRN = 7: Track Status
                        {
                            string ts = Convert2Binary(msgcat20[pos]);
                            string CNF = String.Empty;
                            if (ts[0].Equals('1')) { CNF = "Track initialization phase"; }
                            else { CNF = "Confirmed Track"; }

                            string TRE = String.Empty;
                            if (ts[1].Equals('1')) { TRE = "Last report of track"; }
                            else { TRE = "Default"; }

                            string CST = string.Empty;
                            if (ts[2].Equals('0')) { CST = "No Extrapolated"; }
                            else { CST = "Extrapolated"; }

                            StringBuilder cdm = new StringBuilder(ts[3]);
                            cdm.Append(ts[4]);
                            string CDM = string.Empty;
                            if (cdm.ToString().Equals("0")) { CST = "Maintaining"; }
                            else if (cdm.ToString().Equals("1")) { CST = "Climbing"; }
                            else if (cdm.ToString().Equals("10")) { CST = "Descending"; }
                            else if (cdm.ToString().Equals("11")) { CST = "Invalid"; }

                            string MAH = String.Empty;
                            if (ts[4].Equals('1')) { MAH = "Horizontal manoeuvre"; }
                            else { MAH = "Default"; }

                            string STH = String.Empty;
                            if (ts[6].Equals('1')) { STH = "Smoothed position"; }
                            else { STH = "Measured position"; }

                            string GHO = string.Empty;

                            if (ts[7].Equals('0')) { pos += 1; }
                            else
                            {
                                string ts_1 = Convert2Binary(msgcat20[pos]);

                                if (ts_1[0].Equals('1')) { GHO = "Ghost track"; }
                                else { GHO = "Default"; }

                                pos += 1;
                            }
                            TS = new string[7] { CNF, TRE, CST, CDM, MAH, STH, GHO };
                        }// FRN = 7; Track Status

                        if (FSPEC_1[7] == '0') { }
                        else
                        {
                            string FSPEC_2 = FSPEC_20T[a][1];
                            if (FSPEC_2[0] == '1') // FRN = 8: Mode-3A
                            {
                                string V = string.Empty;
                                string G = string.Empty;
                                string L = string.Empty;
                                string Response = string.Empty;

                                string mode3A = Convert2Binary(msgcat20[pos]);
                                string mode3A_2 = Convert2Binary(msgcat20[pos + 1]);

                                if (mode3A[0].Equals('0')) V = "Code validated";
                                else V = "Code not validated";
                                if (mode3A[1].Equals('0')) G = "Default";
                                else G = "Garbled code";
                                if (mode3A[2].Equals('0')) L = "Mode-3/A code derived from the reply of the transponder";
                                else L = "Mode-3/A code not extracted during the last update period";

                                StringBuilder resp = new StringBuilder();
                                resp.Append(mode3A[4]);
                                resp.Append(mode3A[5]); 
                                resp.Append(mode3A[6]);
                                resp.Append(mode3A[7]);
                                resp.Append(mode3A_2);

                                string resp1 = resp.ToString();
                                int resp2 = (int)Convert.ToInt32(resp1, 2);

                                Response = Convert.ToString(resp2, 8);

                                Mode3A = new string[4] { V, G, L, Response };
                                pos += 2;
                            }// FRN = 8: Mode-3A

                            if (FSPEC_2[1] == '1') // FRN = 9: Cartesian Track Velocity
                            {
                                string Vx1 = Convert2Binary(msgcat20[pos]);
                                string Vx2 = Convert2Binary(msgcat20[pos + 1]);
                                StringBuilder Vx_BIN = new StringBuilder(Vx1);
                                Vx_BIN.Append(Vx2);
                                string Vx_BIN_TOTAL = Vx_BIN.ToString();
                                double Vx = (int)Convert.ToInt64(Vx_BIN_TOTAL, 2); // in m
                                string Vy1 = Convert2Binary(msgcat20[pos + 2]);
                                string Vy2 = Convert2Binary(msgcat20[pos + 3]);
                                StringBuilder Vy_BIN = new StringBuilder(Vy1);
                                Vy_BIN.Append(Vy2);
                                string Vy_BIN_TOTAL = Vy_BIN.ToString();
                                double Vy = ((int)Convert.ToInt64(Vy_BIN_TOTAL, 2)); // in degrees
                                CTV = new double[2] { Vx, Vy };
                                pos += 4;
                            }// FRN = 9: Cartesian Track Velocity

                            if (FSPEC_2[2] == '1') // FRN = 10: Flight Level
                            {

                                string V = "";
                                string G = "";
                                long FL = 0;

                                string fl = Convert2Binary(msgcat20[pos]);
                                string fl_2 = Convert2Binary(msgcat20[pos + 1]);
                                if (fl[0].Equals('0')) V = "Code validated";
                                else V = "Code not validated";
                                if (fl[1].Equals('0')) G = "Default";
                                else G = "Garbled code";


                                StringBuilder resp = new StringBuilder(fl[2]);
                                resp.Append(fl[3]);
                                resp.Append(fl[4]);
                                resp.Append(fl[5]);
                                resp.Append(fl[6]);
                                resp.Append(fl[7]);
                                resp.Append(fl_2);

                                int resp1 = Convert.ToInt32(resp.ToString());

                                string FL1 = Convert2Binary(Convert.ToDouble(resp1));
                                FL = Convert.ToInt64(FL1, 2);
                                FL /= 4;

                                FL_T = new string[3] { V, G, FL.ToString() };
                                pos += 2;

                            }// FRN = 10: Flight Level

                            if (FSPEC_2[3] == '1') // FRN = 11: Mode C code
                            {
                                string V = "";
                                string G = "";
                                string response = "";

                                string modec1 = Convert2Binary(msgcat20[pos]);
                                string modec2 = Convert2Binary(msgcat20[pos + 1]);
                                string modec3 = Convert2Binary(msgcat20[pos + 2]);
                                string modec4 = Convert2Binary(msgcat20[pos + 3]);
                                if (modec1[0].Equals('0')) V = "Code validated";
                                else V = "Code not validated";
                                if (modec1[1].Equals('0')) G = "Default";
                                else G = "Garbled code";


                                StringBuilder resp = new StringBuilder(modec1[4]);
                                resp.Append(modec1[5]);
                                resp.Append(modec1[6]);
                                resp.Append(modec1[7]);
                                resp.Append(modec2);

                                string codeC_BIN = Convert2Binary(Convert.ToDouble(resp));
                                uint codeC_BINA = Convert.ToUInt32(codeC_BIN);
                                uint codeC_GRAY = (codeC_BINA >> 1) ^ codeC_BINA;

                                string codeC = codeC_GRAY.ToString();

                                ModeC = new string[3] { V, G, codeC };
                                pos += 4;
                            }// FRN = 11: Mode C code

                            if (FSPEC_2[4] == '1') // FRN = 12: ICAO address
                            {
                                string ICAO1 = Convert2Binary(msgcat20[pos]);
                                string ICAO2 = Convert2Binary(msgcat20[pos + 1]);
                                string ICAO3 = Convert2Binary(msgcat20[pos + 2]);
                                StringBuilder ICAO = new StringBuilder(ICAO1);
                                ICAO.Append(ICAO2);
                                ICAO.Append(ICAO3);
                                ICAO_Address = ICAO.ToString();

                                pos += 3;
                            }// FRN = 12: ICAO address

                            if (FSPEC_2[5] == '1') // FRN = 13: Target Identification
                            {
                                string sti1 = Convert2Binary(msgcat20[pos]);
                                StringBuilder sti = new StringBuilder(sti1[0]);
                                sti.Append(sti1[1]);
                                string STI = string.Empty;

                                if (sti.ToString().Equals("0")) { STI = "Callsign or registration not downlinked from transponder"; }
                                else if (sti.ToString().Equals("1")) { STI = "Registration downlinked from transponder"; }
                                else if (sti.ToString().Equals("10")) { STI = "Callsign downlinked from transponder"; }
                                else if (sti.ToString().Equals("11")) { STI = "Not defined"; }


                                string byte2 = Convert2Binary(msgcat20[pos + 1]);
                                StringBuilder tid1 = new StringBuilder(byte2[0]);
                                tid1.Append(byte2[0]);
                                tid1.Append(byte2[1]);
                                tid1.Append(byte2[2]);
                                tid1.Append(byte2[3]);
                                tid1.Append(byte2[4]);
                                char TID1 = ConvertToIA5(tid1);

                                string byte3 = Convert2Binary(msgcat20[pos + 2]);
                                StringBuilder tid2 = new StringBuilder(byte2[6]);
                                tid2.Append(byte2[7]);
                                tid2.Append(byte3[0]);
                                tid2.Append(byte3[1]);
                                tid2.Append(byte3[2]);
                                tid2.Append(byte3[3]);
                                char TID2 = ConvertToIA5(tid2);

                                string byte4 = Convert2Binary(msgcat20[pos + 3]);
                                StringBuilder tid3 = new StringBuilder(byte3[4]);
                                tid3.Append(byte3[5]);
                                tid3.Append(byte3[6]);
                                tid3.Append(byte3[7]);
                                tid3.Append(byte4[0]);
                                tid3.Append(byte4[1]);
                                char TID3 = ConvertToIA5(tid3);

                                StringBuilder tid4 = new StringBuilder(byte4[2]);
                                tid4.Append(byte4[3]);
                                tid4.Append(byte4[4]);
                                tid4.Append(byte4[5]);
                                tid4.Append(byte4[6]);
                                tid4.Append(byte4[7]);
                                char TID4 = ConvertToIA5(tid4);

                                string byte5 = Convert2Binary(msgcat20[pos + 4]);
                                StringBuilder tid5 = new StringBuilder(byte5[0]);
                                tid5.Append(byte5[1]);
                                tid5.Append(byte5[2]);
                                tid5.Append(byte5[3]);
                                tid5.Append(byte5[4]);
                                tid5.Append(byte5[5]);
                                char TID5 = ConvertToIA5(tid5);

                                string byte6 = Convert2Binary(msgcat20[pos + 5]);
                                StringBuilder tid6 = new StringBuilder(byte5[6]);
                                tid6.Append(byte5[7]);
                                tid6.Append(byte6[0]);
                                tid6.Append(byte6[1]);
                                tid6.Append(byte6[2]);
                                tid6.Append(byte6[3]);
                                char TID6 = ConvertToIA5(tid6);

                                string byte7 = Convert2Binary(msgcat20[pos + 6]);
                                StringBuilder tid7 = new StringBuilder(byte6[4]);
                                tid7.Append(byte6[5]);
                                tid7.Append(byte6[6]);
                                tid7.Append(byte6[7]);
                                tid7.Append(byte7[0]);
                                tid7.Append(byte7[1]);
                                char TID7 = ConvertToIA5(tid7);

                                StringBuilder tid8 = new StringBuilder(byte7[2]);
                                tid8.Append(byte7[3]);
                                tid8.Append(byte7[4]);
                                tid8.Append(byte7[5]);
                                tid8.Append(byte7[6]);
                                tid8.Append(byte7[7]);
                                char TID8 = ConvertToIA5(tid8);

                                string TID_T = string.Concat(TID1, TID2, TID3, TID4, TID5, TID6, TID7, TID8);
                                TID = new string[2] { STI, TID_T };
                                pos += 7;
                            }// FRN = 13: Target Identification

                            if (FSPEC_2[6] == '1') // FRN = 14: Measured heigh in cartesian coordinates
                            {
                                string h1 = Convert2Binary(msgcat20[pos]);
                                string h2 = Convert2Binary(msgcat20[pos + 1]);
                                StringBuilder h = new StringBuilder(h1);
                                h.Append(h2);
                                cartesianH = Convert.ToDouble(h);

                                pos += 2;
                            }// FRN = 14: Measured heigh in cartesian coordinates

                            if (FSPEC_2[7] == '0') { }
                            else
                            {
                                string FSPEC_3 = FSPEC_20T[a][2];
                                if (FSPEC_3[0] == '1') // FRN = 15: Geometric Heigh (WGS-84)
                                {
                                    string h1 = Convert2Binary(msgcat20[pos]);
                                    string h2 = Convert2Binary(msgcat20[pos + 1]);
                                    StringBuilder h = new StringBuilder(h1);
                                    h.Append(h2);
                                    geometricH = Convert.ToDouble(h);

                                    pos += 2;
                                }// FRN = 15: Geometric Heigh (WGS-84)

                                if (FSPEC_3[1] == '1') // FRN = 16: Calculated acceleration
                                {
                                    string ax1 = Convert2Binary(msgcat20[pos]);
                                    double ax = (int)Convert.ToInt64(ax1, 2);
                                    ax /= 4; // in m/s^2
                                    string ay1 = Convert2Binary(msgcat20[pos + 1]);
                                    double ay = ((int)Convert.ToInt64(ay1, 2));
                                    ay /= 4;  // in m/s^2
                                    CA = new double[2] { ax, ay };
                                    pos += 2;
                                }// FRN = 16: Calculated acceleration

                                if (FSPEC_3[2] == '1') // FRN = 17: Vehicle Fleet Identification
                                {
                                    string vfi = msgcat20[pos].ToString();
                                    switch (vfi)
                                    {

                                        case "0":
                                            VFI = "Unknown";
                                            break;
                                        case "1":
                                            VFI = "ATC equipment maintenance";
                                            break;
                                        case "2":
                                            VFI = "Airport maintentance";
                                            break;
                                        case "3":
                                            VFI = "Fire";
                                            break;
                                        case "4":
                                            VFI = "Bird scarer";
                                            break;
                                        case "5":
                                            VFI = "Snow plough";
                                            break;
                                        case "6":
                                            VFI = "Runway sweeper";
                                            break;
                                        case "7":
                                            VFI = "Emergency";
                                            break;
                                        case "8":
                                            VFI = "Police";
                                            break;
                                        case "9":
                                            VFI = "Bus";
                                            break;
                                        case "10":
                                            VFI = "Tug (push/tow)";
                                            break;
                                        case "11":
                                            VFI = "Grass cutter";
                                            break;
                                        case "12":
                                            VFI = "Fuel";
                                            break;
                                        case "13":
                                            VFI = "Baggage";
                                            break;
                                        case "14":
                                            VFI = "Catering";
                                            break;
                                        case "15":
                                            VFI = "Aircraft maintenance";
                                            break;
                                        case "16":
                                            VFI = "Flyco (follow me)";
                                            break;
                                    }
                                    pos += 1;
                                } // FRN = 17: Vehicle Fleet Identification

                                if (FSPEC_3[3] == '1') // FRN = 18: Pre-programmed message
                                {
                                    string ppm = msgcat20[pos].ToString();
                                    char trb = ppm[0];
                                    string TRB = string.Empty;
                                    if (trb.Equals('0')) { TRB = "Default"; }
                                    else { TRB = "In Trouble"; }

                                    StringBuilder msgt = new StringBuilder(ppm[1]);
                                    msgt.Append(ppm[2]);
                                    msgt.Append(ppm[3]);
                                    msgt.Append(ppm[4]);
                                    msgt.Append(ppm[5]);
                                    msgt.Append(ppm[6]);
                                    msgt.Append(ppm[7]);

                                    string msg = msgt.ToString();
                                    string MSG = string.Empty;

                                    switch (msg)
                                    {

                                        case "0000001":
                                            MSG = "Towing aircraft";
                                            break;
                                        case "0000010":
                                            MSG = "'Follow me' operations";
                                            break;
                                        case "0000011":
                                            MSG = "Runway check";
                                            break;
                                        case "0000100":
                                            MSG = "Emergency operation (fire, medical...)";
                                            break;
                                        case "0000101":
                                            MSG = "Work in progress (maintenance, birds scarer, sweepers...)";
                                            break;
                                    }
                                    pos += 1;
                                    PPM = new string[2] { TRB, MSG };
                                } // FRN = 18: Pre-programmed message

                                if (FSPEC_3[4] == '1') //FRN = 19: Position accuracy
                                {
                                    string accuracy = Convert2Binary(msgcat20[pos]);
                                    pos += 1;
                                    if (accuracy[0].Equals('1'))
                                    {
                                        string dopx1 = Convert2Binary(msgcat20[pos]);
                                        string dopx2 = Convert2Binary(msgcat20[pos + 1]);
                                        StringBuilder dopx_bin = new StringBuilder(dopx1);
                                        dopx_bin.Append(dopx2);
                                        double DOPx = (Convert.ToDouble(dopx_bin.ToString())) * 0.25;

                                        string dopy1 = Convert2Binary(msgcat20[pos + 2]);
                                        string dopy2 = Convert2Binary(msgcat20[pos + 3]);
                                        StringBuilder dopy_bin = new StringBuilder(dopy1);
                                        dopy_bin.Append(dopy2);
                                        double DOPy = (Convert.ToDouble(dopy_bin.ToString())) * 0.25;

                                        string dopxy1 = Convert2Binary(msgcat20[pos + 4]);
                                        string dopxy2 = Convert2Binary(msgcat20[pos + 5]);
                                        StringBuilder dopxy_bin = new StringBuilder(dopxy1);
                                        dopxy_bin.Append(dopxy2);
                                        double DOPxy = (Convert.ToDouble(dopxy_bin.ToString())) * 0.25;

                                        DOP = new double[3] { DOPx, DOPy, DOPxy };
                                        pos += 6;

                                    }

                                    if (accuracy[1].Equals('1'))
                                    {
                                        string sigmax1 = Convert2Binary(msgcat20[pos]);
                                        string sigmax2 = Convert2Binary(msgcat20[pos + 1]);
                                        StringBuilder sigmax_bin = new StringBuilder(sigmax1);
                                        sigmax_bin.Append(sigmax2);
                                        double SIGMAx = (Convert.ToDouble(sigmax_bin.ToString())) * 0.25;

                                        string sigmay1 = Convert2Binary(msgcat20[pos + 2]);
                                        string sigmay2 = Convert2Binary(msgcat20[pos + 3]);
                                        StringBuilder sigmay_bin = new StringBuilder(sigmay1);
                                        sigmax_bin.Append(sigmay2);
                                        double SIGMAy = (Convert.ToDouble(sigmay_bin.ToString())) * 0.25;

                                        string rhoxy1 = Convert2Binary(msgcat20[pos + 4]);
                                        string rhoxy2 = Convert2Binary(msgcat20[pos + 5]);
                                        StringBuilder rhoxy_bin = new StringBuilder(rhoxy1);
                                        rhoxy_bin.Append(rhoxy2);
                                        double RHOxy = Convert.ToDouble(rhoxy_bin.ToString()) * 0.25;

                                        SDEV = new double[3] { SIGMAx, SIGMAy, RHOxy };
                                        pos += 6;
                                    }

                                    if (accuracy[2].Equals('1'))
                                    {
                                        string sigmaGH1 = Convert2Binary(msgcat20[pos]);
                                        string sigmaGH2 = Convert2Binary(msgcat20[pos + 1]);
                                        StringBuilder sigmaGH = new StringBuilder(sigmaGH1);
                                        sigmaGH.Append(sigmaGH2);

                                        SIGMA_GH = (Convert.ToDouble(sigmaGH.ToString())) * 0.5;
                                        pos += 2;
                                    }

                                }//FRN = 19: Position accuracy

                                if (FSPEC_3[5] == '1') //FRN = 20: Contributing devices
                                {
                                    string REP = msgcat20[pos].ToString();
                                    string[] CTRU = new string[8];
                                    string TRx = msgcat20[pos + 1].ToString();
                                    int n = 0;
                                    while (n < TRx.Length)
                                    {
                                        if (TRx[n].Equals('1'))
                                            CTRU[n] = "TUx/RUx number " + n + " has contributed to the target detection";
                                        else
                                            CTRU[n] = "TUx/RUx number " + n + " has NOT contributed to the target detection";
                                        n += 1;
                                    }
                                    pos += 2;
                                    CD = new string[2] { REP, TRx };
                                }//FRN = 20: Contributing devices
                            }
                        }
                    }
                    else if (CAT1920[a].Equals(1))
                    {
                        CAT = "19";
                        if (FSPEC_1[0] == '1')// FRN = 1: Data Source ID
                        {
                            SAC = Convert.ToInt32(msgcat20[pos]);
                            SIC = Convert.ToInt32(msgcat20[pos + 1]);
                            pos = pos + 2;
                        }// FRN = 1: Data Source ID

                        if (FSPEC_1[1] == '1')// FRN = 2: Message Type
                        {
                            double val = msgcat20[pos];
                            switch (val)
                            {
                                case 1:
                                    MsgType = "Target Report";
                                    break;
                                case 2:
                                    MsgType = "Start of Update Cycle";
                                    break;
                                case 3:
                                    MsgType = "Periodic Status Message";
                                    break;
                                case 4:
                                    MsgType = "Event-Triggered Status Message";
                                    break;
                            }
                            pos += 1;
                        }// FRN = 2: Message Type

                        if (FSPEC_1[2] == '1') //FRN = 3: Time Of Day
                        {
                            string a1 = Convert2Binary(msgcat20[pos]);
                            string a2 = Convert2Binary(msgcat20[pos + 1]);
                            string a3 = Convert2Binary(msgcat20[pos + 2]);
                            StringBuilder hour = new StringBuilder(a1);
                            hour.Append(a2);
                            hour.Append(a3);
                            string hour_in_seconds = hour.ToString();
                            int Hour = (int)Convert.ToInt64(hour_in_seconds, 2);
                            Hour /= 128;
                            TimeOfDay = TimeSpan.FromSeconds(Hour); // hh:mm:ss
                            pos += 3;
                        }//FRN = 3: Time Of Day
                    }

                    CAT20 obj = new CAT20(CAT, SIC, SAC, TRD, MsgType, TimeOfDay, CP, TN, TS, Mode3A, CTV, FL_T, ModeC, ICAO_Address, TID, cartesianH, geometricH, CA, VFI, PPM, DOP, SDEV, SIGMA_GH, CD);

                    listCAT20.Add(obj);
                    a += 1;
                }
            }
            return listCAT20;
        }

        public List<CAT21> ReadCat21(List<double[]> msgcat21_T, List<string[]> FSPEC_21T)
        {
            int a = 0;
            int SAC = 0; int SIC = 0;
            double GH = 0.0;
            string ICAO_Address = string.Empty; string FL_T = string.Empty; string TID = string.Empty; string VA = string.Empty;
            TimeSpan TimeOfDay = TimeSpan.Zero;
            string[] TRD = Array.Empty<string>(); string[] FOM = Array.Empty<string>(); string[] LTI = Array.Empty<string>();
            double[] WGS84 = Array.Empty<double>(); double[] AGV = Array.Empty<double>(); 

            List<CAT21> listCAT21 = new List<CAT21>();

            if (msgcat21_T != null && FSPEC_21T != null)
            {
                while (a < msgcat21_T.Count)
                {

                    string FSPEC_1 = FSPEC_21T[a][0];
                    double[] msgcat21 = msgcat21_T[a];
                    // int n = 0;
                    int pos = FSPEC_21T[a].Length; // posició de byte en el missatge rebut de categoria 20 SENSE cat,lenght,Fspec.

                    if (FSPEC_1[0] == '1')// FRN = 1: Data Source ID
                    {
                        SAC = Convert.ToInt32(msgcat21[pos]);
                        SIC = Convert.ToInt32(msgcat21[pos + 1]);
                        pos = pos + 2;
                    }// FRN = 1: Data Source ID

                    if (FSPEC_1[1] == '1')// FRN = 2: Target Report Description
                    {
                        string va = Convert2Binary(msgcat21[pos]);

                        string DCR = String.Empty;
                        if (va[0].Equals('0')) { DCR = "No differential correction (ADS-B)"; }
                        else { DCR = "Differential correction (ADS-B)"; }

                        string GBS = String.Empty;
                        if (va[1].Equals('0')) { GBS = "Transponder Ground Bit Not Set"; }
                        else { GBS = "Transponder Ground Bit Set"; }

                        string SIM = String.Empty;
                        if (va[2].Equals('0')) { SIM = "Actual Target Report"; }
                        else { SIM = "Simulated Target Report"; }

                        string TST = String.Empty;
                        if (va[3].Equals('0')) { TST = "Default"; }
                        else { TST = "Test Target"; }

                        string RAB = String.Empty;
                        if (va[4].Equals('0')) { RAB = "Report from Target Responder"; }
                        else { RAB = "Report From Field Monitor (fixed transpoder)"; }

                        string SAA = String.Empty;
                        if (va[5].Equals('0')) { SAA = "Equipement not capable to provide Selected Altitude"; }
                        else { SAA = "Equipement  capable to provide Selected Altitude"; }

                        string SPI = String.Empty;
                        if (va[6].Equals('0')) { SPI = "Absence of SPI"; }
                        else { SPI = "Special Position Identification"; }

                        va = Convert2Binary(msgcat21[pos + 1]);

                        StringBuilder atp = new StringBuilder(va[0]);
                        atp.Append(va[1]);
                        atp.Append(va[2]);
                        string ATP;
                        if (atp.ToString().Equals("00"))
                        {
                            ATP = "Non unique address";
                        }
                        else if (atp.ToString().Equals("01"))
                        {
                            ATP = "24-bit ICAO address";
                        }
                        else if (atp.ToString().Equals("10"))
                        {
                            ATP = "Surface vehicle address";
                        }
                        else if (atp.ToString().Equals("11"))
                        {
                            ATP = "Annonymous address";
                        }
                        else
                        {
                            ATP = "Reserved for future use";
                        }

                        pos += 2;

                        TRD = new string[8] { DCR, GBS, SIM, TST, RAB, SAA, SPI, ATP};

                    }// FRN = 2: Target Report Description

                    if (FSPEC_1[2] == '1') //FRN = 3: Time Of Day
                    {
                        string a1 = Convert2Binary(msgcat21[pos]);
                        string a2 = Convert2Binary(msgcat21[pos + 1]);
                        string a3 = Convert2Binary(msgcat21[pos + 2]);
                        StringBuilder hour = new StringBuilder(a1);
                        hour.Append(a2);
                        hour.Append(a3);
                        string hour_in_seconds = hour.ToString();
                        int Hour = (int)Convert.ToInt64(hour_in_seconds, 2);
                        Hour /= 128;
                        TimeOfDay = TimeSpan.FromSeconds(Hour); // hh:mm:ss
                        pos += 3;
                    }//FRN = 3: Time Of Day

                    if (FSPEC_1[3] == '1')//FRN = 4: WGS84_Position
                    {
                        string lat1 = Convert2Binary(msgcat21[pos]);
                        string lat2 = Convert2Binary(msgcat21[pos + 1]);
                        string lat3 = Convert2Binary(msgcat21[pos + 2]);
                        StringBuilder lat = new StringBuilder(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1[0]);
                        lat.Append(lat1);
                        lat.Append(lat2);
                        lat.Append(lat3);
                        int latint = Convert.ToInt32(lat.ToString(), 2);
                        double latdouble = Convert.ToDouble(latint);
                        double LAT = (latdouble * 180) / 8388608;


                        //double LAT = Convert.ToDouble(Convert.ToInt32(lat.ToString(), 2)) * (180 / (2 ^ 23));

                        string lon1 = Convert2Binary(msgcat21[pos + 3]);
                        string lon2 = Convert2Binary(msgcat21[pos + 4]);
                        string lon3 = Convert2Binary(msgcat21[pos + 5]);
                        StringBuilder lon = new StringBuilder(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1[0]);
                        lon.Append(lon1);
                        lon.Append(lon2);
                        lon.Append(lon3);

                        int lonint = Convert.ToInt32(lon.ToString(), 2);
                        double londouble = Convert.ToDouble(lonint);
                        double LON = (londouble * 180) / 8388608;

                        WGS84 = new double[2] { LAT, LON };

                        pos += 6;
                    } //FRN = 4: WGS84_Position

                    if (FSPEC_1[4] == '1') // FRN = 5: TargetAddress
                    {
                        string ICAO1 = Convert2Binary(msgcat21[pos]);
                        string ICAO2 = Convert2Binary(msgcat21[pos + 1]);
                        string ICAO3 = Convert2Binary(msgcat21[pos + 2]);
                        StringBuilder ICAO = new StringBuilder(ICAO1);
                        ICAO.Append(ICAO2);
                        ICAO.Append(ICAO3);
                        ICAO_Address = ICAO.ToString();
                        pos += 3;
                    } // FRN = 5: TargetAddress

                    if (FSPEC_1[5] == '1') { pos += 2; } // FRN = 6: Geometric Altitude

                    if (FSPEC_1[6] == '1')// FRN = 7; Figure of Merit
                    {

                        string fom1 = Convert2Binary(msgcat21[pos]);
                        string fom2 = Convert2Binary(msgcat21[pos + 1]);

                        StringBuilder ac = new StringBuilder(fom1[0]);
                        ac.Append(fom1[1]);
                        string AC = string.Empty;
                        if (ac.ToString().Equals("0")) { AC = "Unknown"; }
                        else if (ac.ToString().Equals("01")) { AC = "ACAS not operational"; }
                        else if (ac.ToString().Equals("10")) { AC = "ACAS operational"; }
                        else if (ac.ToString().Equals("11")) { AC = "Invalid"; }

                        StringBuilder mn = new StringBuilder(fom1[2]);
                        mn.Append(fom1[3]);
                        string MN = string.Empty;
                        if (mn.ToString().Equals("0")) { MN = "Unknown"; }
                        else if (mn.ToString().Equals("01")) { MN = "Multiple navigational aids not operating"; }
                        else if (mn.ToString().Equals("10")) { MN = "Multiple navigational aids operating"; }
                        else if (mn.ToString().Equals("11")) { MN = "Invalid"; }

                        StringBuilder dc = new StringBuilder(fom1[2]);
                        dc.Append(fom1[3]);
                        string DC = string.Empty;
                        if (dc.ToString().Equals("0")) { DC = "Unknown"; }
                        else if (dc.ToString().Equals("01")) { DC = "Differential correction"; }
                        else if (dc.ToString().Equals("10")) { DC = "No differential correction"; }
                        else if (dc.ToString().Equals("11")) { DC = "Invalid"; }

                        StringBuilder pa = new StringBuilder(fom2[4]);
                        pa.Append(fom2[5]);
                        pa.Append(fom2[6]);
                        pa.Append(fom2[7]);

                        string PA = pa.ToString();

                        FOM = new string[4] { AC, MN, DC, PA };


                        pos += 2;
                    } // FRN = 7; Figure of Merit


                    if (FSPEC_1[7] == '0') { }
                    else
                    {
                        string FSPEC_2 = FSPEC_21T[a][1];

                        if (FSPEC_2[0] == '1')// FRN = 8: Link Technology Indicator
                        {
                            string lti = Convert2Binary(msgcat21[pos]);
                            string DTI; string MDS; string UAT; string VDL; string OTR;

                            if(lti[3].Equals('0')) { DTI = "Unknown"; }
                            else { DTI = "Aircraft equiped with CDTI"; }

                            if (lti[4].Equals('0')) { MDS = "Not used"; }
                            else { MDS = "Used"; }

                            if (lti[5].Equals('0')) { UAT = "Not used"; }
                            else { UAT = "Used"; }

                            if (lti[6].Equals('0')) { VDL = "Not used"; }
                            else { VDL = "Used"; }

                            if (lti[7].Equals('0')) { OTR = "Not used"; }
                            else { OTR = "Used"; }

                            LTI = new string[5] { DTI, MDS, UAT, VDL, OTR };

                            pos += 1;
                        }// FRN = 8: Link Technology Indicator

                        if (FSPEC_2[1] == '1') { pos += 2; } // FRN = 9: Roll angle

                        if (FSPEC_2[2] == '1') // FRN = 10: Flight Level
                        {
                            int FL = 0;

                            string fl = Convert2Binary(msgcat21[pos]);
                            string fl_2 = Convert2Binary(msgcat21[pos + 1]);

                            StringBuilder resp = new StringBuilder(fl);
                            resp.Append(fl_2);

                            string FL1 = resp.ToString();
                            FL = Convert.ToInt32(FL1, 2);
                            FL /= 4;

                            FL_T = FL.ToString();
                            pos += 2;

                        }// FRN = 10: Flight Level

                        if (FSPEC_2[3] == '1') { pos += 2; } // FRN = 11: Air Speed
                        if (FSPEC_2[4] == '1') { pos += 2; }// FRN = 12: True Air Speed
                        if (FSPEC_2[5] == '1') { pos += 2; }// FRN = 13: Magnetic Heading
                        if (FSPEC_2[6] == '1') { pos += 2; }// FRN = 14: Barometric vertical rate

                        if (FSPEC_2[7] == '0') { }
                        else
                        {
                            string FSPEC_3 = FSPEC_21T[a][2];

                            if (FSPEC_3[0] == '1') // FRN = 15: Geometric Vertical Rate (WGS-84)
                            {
                                string gh1 = Convert2Binary(msgcat21[pos]);
                                string gh2 = Convert2Binary(msgcat21[pos + 1]);

                                StringBuilder gh = new StringBuilder(gh1);
                                gh.Append(gh2);
                                GH = Convert.ToDouble(Convert.ToInt16(gh.ToString(), 2)) * 6.25;
                                pos += 2;
                            } // FRN = 15: Geometric Vertical Rate (WGS-84)

                            if (FSPEC_3[1] == '1') // FRN = 16: Airborne ground vector
                            {

                                string ground_speed1 = Convert2Binary(msgcat21[pos]);
                                string ground_speed2 = Convert2Binary(msgcat21[pos + 1]);
                                StringBuilder ground_speed_BIN = new StringBuilder(ground_speed1);
                                ground_speed_BIN.Append(ground_speed2);
                                string ground_speed_BIN_TOTAL = ground_speed_BIN.ToString();
                                double ground_speed = ((int)Convert.ToInt16(ground_speed_BIN_TOTAL, 2)) * 0.22; // in m
                                string track_angle1 = Convert2Binary(msgcat21[pos + 2]);
                                string track_angle2 = Convert2Binary(msgcat21[pos + 3]);
                                StringBuilder track_angle_BIN = new StringBuilder(track_angle1);
                                track_angle_BIN.Append(track_angle2);
                                string track_angle_BIN_TOTAL = track_angle_BIN.ToString();
                                int ta_int = Convert.ToInt16(track_angle_BIN_TOTAL, 2);
                                double track_angle =  ta_int * 360 / 65536; // in degrees
                                AGV = new double[2] { ground_speed, track_angle };

                                pos += 4;
                            }// FRN = 16: Airborne ground vector

                            if (FSPEC_3[2] == '1')// FRN = 17: Rate of turn
                            {
                                string rot = Convert2Binary(msgcat21[pos]);
                                pos += 1;
                                if (rot[7].Equals('0')) { }
                                else
                                {
                                    string rot2 = Convert2Binary(msgcat21[pos]);
                                    pos += 1;
                                    if (rot2[7].Equals('0')) { }
                                    else
                                    {
                                        string rot3 = Convert2Binary(msgcat21[pos]);
                                        pos += 1;

                                    }
                                }
                            }// FRN = 17: Rate of turn

                            if (FSPEC_3[3] == '1') // FRN = 18: Target Identification
                            {

                                string byte1 = Convert2Binary(msgcat21[pos]);
                                StringBuilder tid1 = new StringBuilder();
                                tid1.Append(byte1[0]);
                                tid1.Append(byte1[1]);
                                tid1.Append(byte1[2]);
                                tid1.Append(byte1[3]);
                                tid1.Append(byte1[4]);
                                tid1.Append(byte1[5]);
                                char TID1 = ConvertToIA5(tid1);

                                string byte2 = Convert2Binary(msgcat21[pos + 1]);
                                StringBuilder tid2 = new StringBuilder();
                                tid2.Append(byte1[6]);
                                tid2.Append(byte1[7]);
                                tid2.Append(byte2[0]);
                                tid2.Append(byte2[1]);
                                tid2.Append(byte2[2]);
                                tid2.Append(byte2[3]);
                                char TID2 = ConvertToIA5(tid2);

                                string byte3 = Convert2Binary(msgcat21[pos + 2]);
                                StringBuilder tid3 = new StringBuilder();
                                tid3.Append(byte2[4]);
                                tid3.Append(byte2[5]);
                                tid3.Append(byte2[6]);
                                tid3.Append(byte2[7]);
                                tid3.Append(byte3[0]);
                                tid3.Append(byte3[1]);
                                char TID3 = ConvertToIA5(tid3);

                                StringBuilder tid4 = new StringBuilder();
                                tid4.Append(byte3[2]);
                                tid4.Append(byte3[3]);
                                tid4.Append(byte3[4]);
                                tid4.Append(byte3[5]);
                                tid4.Append(byte3[6]);
                                tid4.Append(byte3[7]);
                                char TID4 = ConvertToIA5(tid4);

                                string byte4 = Convert2Binary(msgcat21[pos + 3]);
                                StringBuilder tid5 = new StringBuilder();
                                tid5.Append(byte4[0]);
                                tid5.Append(byte4[1]);
                                tid5.Append(byte4[2]);
                                tid5.Append(byte4[3]);
                                tid5.Append(byte4[4]);
                                tid5.Append(byte4[5]);
                                char TID5 = ConvertToIA5(tid5);

                                string byte5 = Convert2Binary(msgcat21[pos + 4]);
                                StringBuilder tid6 = new StringBuilder();
                                tid6.Append(byte4[6]);
                                tid6.Append(byte4[7]);
                                tid6.Append(byte5[0]);
                                tid6.Append(byte5[1]);
                                tid6.Append(byte5[2]);
                                tid6.Append(byte5[3]);
                                char TID6 = ConvertToIA5(tid6);

                                string byte6 = Convert2Binary(msgcat21[pos + 5]);
                                StringBuilder tid7 = new StringBuilder();
                                tid7.Append(byte5[4]);
                                tid7.Append(byte5[5]);
                                tid7.Append(byte5[6]);
                                tid7.Append(byte5[7]);
                                tid7.Append(byte6[0]);
                                tid7.Append(byte6[1]);
                                char TID7 = ConvertToIA5(tid7);

                                StringBuilder tid8 = new StringBuilder();
                                tid8.Append(byte6[2]);
                                tid8.Append(byte6[3]);
                                tid8.Append(byte6[4]);
                                tid8.Append(byte6[5]);
                                tid8.Append(byte6[6]);
                                tid8.Append(byte6[7]);
                                char TID8 = ConvertToIA5(tid8);

                                TID = string.Concat(TID1, TID2, TID3, TID4, TID5, TID6, TID7, TID8);

                                pos += 6;
                            }// FRN = 18: Target Identification

                            if (FSPEC_3[4] == '1') //FRN = 19: Velocity accuracy
                            {
                                VA = Convert2Binary(msgcat21[pos]);
                                pos += 1;
                            } //FRN = 19: Velocity accuracy
                        }
                    }

                    CAT21 obj = new CAT21(SAC, SIC, TimeOfDay, TRD, ICAO_Address, FOM, VA, WGS84, FL_T, GH, AGV, TID, LTI);

                    listCAT21.Add(obj);
                    a += 1;
                }
            }
            return listCAT21;
        }

        public List<Flight> DistributeFlights(List<CAT10> cat10, List<CAT20> cat20, List<CAT21> cat21)
        {
            List<Flight> listFlights = new List<Flight>();
            int a = 0;
            if (cat10 != null)
            {
                foreach (CAT10 flight in cat10)
                {
                    Flight f = new Flight();
                    f.ID = flight.SIC.ToString();
                    f.TimeofDay = flight.TimeofDay;
                    f.CartesianPosition = flight.CartesianPosition;
                    listFlights.Add(f);
                    a += 1;
                }
            }
            if (cat20 != null)
            {
                foreach (CAT20 flight in cat20)
                {
                    if (flight.CAT.Equals("20")) {
                        Flight f = new Flight();
                        f.ID = flight.SIC.ToString();
                        f.TimeofDay = flight.TimeofDay;
                        f.CartesianPosition = flight.CartesianPosition;
                        listFlights.Add(f);
                        a += 1;
                    }
                }
            }
            if (cat21 != null)
            {
                foreach (CAT21 flight in cat21)
                {
                    Flight f = new Flight();
                    f.ID = flight.SIC.ToString();
                    f.TimeofDay = flight.TimeofDay;
                    f.CartesianPosition = flight.PositionWGS84;
                    listFlights.Add(f);
                    a += 1;
                }
            }
            //List<Flight> listFlightsFinal = ordenar(listFlights);
            return listFlights;
        }

        public string Convert2Binary(double input)
        {
            int n;
            int[] a = new int[8];
            string b = null;
            try
            {
                n = Convert.ToInt32(input);
            }
            catch (OverflowException)
            {
                b = input.ToString();
                return b;
            }
            if (input.Equals(0))
                b = "00000000";
            else
            {
                for (int i = 0; i <= 7; i++)
                {
                    a[7 - i] = n % 2;
                    n = n / 2;
                }
                for (int i = 0; i <= 7; i++)
                {
                    b += Convert.ToString(a[i]);
                }
            }
            return b;
        }

        public string ConvertTime(Int32 tsegundos)
        {
            Int32 horas = (tsegundos / 3600);
            Int32 minutos = ((tsegundos - horas * 3600) / 60);
            Int32 segundos = tsegundos - (horas * 3600 + minutos * 60);
            horas = horas % 24;
            string h = horas.ToString().PadLeft(2, '0');
            string m = minutos.ToString().PadLeft(2, '0');
            string s = segundos.ToString().PadLeft(2, '0');

            return h + ":" + m + ":" + s;
        }

        public string ConvertToBit(string c)
        {
            int n;
            n = Convert.ToInt32(c);
            int[] a = new int[8];
            string b = null;
            for (int i = 0; i <= 7; i++)
            {
                a[7 - i] = n % 2;
                n = n / 2;
            }
            for (int i = 0; i <= 7; i++)
            {
                b = b + Convert.ToString(a[i]);
            }
            return b;
        }

        public char ConvertToIA5(StringBuilder Code)
        {
            char letter = '\0';
            if (Code != null)
            {
                string code = Code.ToString();

                if (code.Equals("000001"))
                {
                    letter = 'A';
                }
                else if (code.Equals("000010"))
                {
                    letter = 'B';
                }
                else if (code.Equals("000011"))
                {
                    letter = 'C';
                }
                else if (code.Equals("000100"))
                {
                    letter = 'D';
                }
                else if (code.Equals("000101"))
                {
                    letter = 'E';
                }
                else if (code.Equals("000110"))
                {
                    letter = 'F';
                }
                else if (code.Equals("000111"))
                {
                    letter = 'G';
                }
                else if (code.Equals("001000"))
                {
                    letter = 'H';
                }
                else if (code.Equals("001001"))
                {
                    letter = 'I';
                }
                else if (code.Equals("001010"))
                {
                    letter = 'J';
                }
                else if (code.Equals("001011"))
                {
                    letter = 'K';
                }
                else if (code.Equals("001100"))
                {
                    letter = 'L';
                }
                else if (code.Equals("001101"))
                {
                    letter = 'M';
                }
                else if (code.Equals("001110"))
                {
                    letter = 'N';
                }
                else if (code.Equals("001111"))
                {
                    letter = 'O';
                }
                else if (code.Equals("010000"))
                {
                    letter = 'P';
                }
                else if (code.Equals("010001"))
                {
                    letter = 'Q';
                }
                else if (code.Equals("010010"))
                {
                    letter = 'R';
                }
                else if (code.Equals("010011"))
                {
                    letter = 'S';
                }
                else if (code.Equals("010100"))
                {
                    letter = 'T';
                }
                else if (code.Equals("010101"))
                {
                    letter = 'U';
                }
                else if (code.Equals("010110"))
                {
                    letter = 'V';
                }
                else if (code.Equals("010111"))
                {
                    letter = 'W';
                }
                else if (code.Equals("011000"))
                {
                    letter = 'X';
                }
                else if (code.Equals("011001"))
                {
                    letter = 'Y';
                }
                else if (code.Equals("011010"))
                {
                    letter = 'Z';
                }
                else if (code.Equals("100000"))
                {
                    letter = ' ';
                }
                else if (code.Equals("110000"))
                {
                    letter = '0';
                }
                else if (code.Equals("110001"))
                {
                    letter = '1';
                }
                else if (code.Equals("110010"))
                {
                    letter = '2';
                }
                else if (code.Equals("110011"))
                {
                    letter = '3';
                }
                else if (code.Equals("110100"))
                {
                    letter = '4';
                }
                else if (code.Equals("110101"))
                {
                    letter = '5';
                }
                else if (code.Equals("110110"))
                {
                    letter = '6';
                }
                else if (code.Equals("110111"))
                {
                    letter = '7';
                }
                else if (code.Equals("111000"))
                {
                    letter = '8';
                }
                else if (code.Equals("111001"))
                {
                    letter = '9';
                }
            }

            return letter;
        }
    }
}

