﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;


namespace CS363_TeamP
{
    class Plane
    {
        public int speed;
        public string ID;
        public char control = ' ';
        public int altitude;
        public int heading;
        public string destAP;
        public int expectedSpeed;
        public int expectedAltitude;
        public int expectedHeading;
        public bool landing = false;
        PictureBox Airplane = new PictureBox();
        TableLayoutPanel tblPlaneInfo;
        Label planeinfo = new Label();
        Bitmap bmp;
        Timer tm = new Timer();
        int x = 100;
        int y = 100;
        Form1 f;



        public void mkPlane(Form1 form, int startLoc)
        {
            f = form;
            //Assign starting location based on mouse click location
            if (startLoc == 1)
            {
                Airplane.Location = new Point(0, 0); //startLoc 1
                planeinfo.Location = new Point(25, 0);
                heading = 121;
            }
            else if (startLoc == 2)
            {
                Airplane.Location = new Point(f.ClientSize.Width-15, -5); //startLoc 2
                planeinfo.Location = new Point(f.ClientSize.Width+10, -5);
                heading = 254;
            }
            else if (startLoc == 3)
            {
                Airplane.Location = new Point(f.ClientSize.Width-310,f.ClientSize.Height); //startLoc 3
                planeinfo.Location = new Point(f.ClientSize.Width - 285, f.ClientSize.Height);
                heading = 5;
            }
            else  //Plane is departing
            {
                Airplane.Location = new Point(402, 277); //startLoc 4
                planeinfo.Location = new Point(427, 277);
                heading = 220;
                control = 'D';
            }
            //Generate random airplane info
            infoGenerator(); 
            //Generate and size bitmap
            bmp = new Bitmap("C:\\Users\\patri\\Source\\Repos\\CS363\\CS363_TeamP\\Resources\\planeIconSmall.png");
            int dpi = 96;
            bmp.SetResolution(dpi, dpi);
            //Assign airplane picture box attributes
            Airplane.ClientSize = new Size(25,25);
            Airplane.BackColor = Color.Transparent;
            Airplane.ForeColor = Color.Red;
            Airplane.Tag = ID;
            Airplane.Image = rotateImage(bmp, heading);
            Airplane.SizeMode = PictureBoxSizeMode.StretchImage;
            
            //Generate eventHandler for clicking on Airplane
            Airplane.Click += new EventHandler(this.Airplane_Click);
            //Add Airplane to form1
            form.Controls.Add(Airplane);
            //Generate plane info label
            planeinfo.AutoSize = true;
            planeinfo.Text = String.Format("{0} {1} {2}" + Environment.NewLine + "{3} {4} {5}", Airplane.Tag, destAP, control, altitude, speed, heading);
            planeinfo.Size = new System.Drawing.Size(198, 60);
            planeinfo.Tag = ID;
            planeinfo.BackColor = Color.Transparent;
            planeinfo.ForeColor = Color.White;
            planeinfo.BorderStyle = BorderStyle.None;
            //Add lable to form1
            form.Controls.Add(planeinfo);
            //Instantiate new timer for plane
            tm.Interval = 1000;
            tm.Tick += new EventHandler(tm_Tick);
            tm.Start();
        }

        private void infoGenerator() 
        {
            //Variables needed for generating random info
            var random = new Random();
            var list = new List<string> { "N", "P", "DL", "AA", "NZ", "UA", "FX", "F9", "KL", "NK" };
            var APlist = new List<string> { "ORD", "LAX", "SFO", "ELP", "GFK", "JFK", "HNL", "BWI", "HOU", "SLC" };
            //Generate random Airline ID
            int index = random.Next(list.Count);
            ID = list[index];
            for (int i=ID.Length; i<6; i++)
            {
                ID += random.Next(9);
            }
            //Generate random destination Airport 
            if (control == 'D')
            {
                index = random.Next(APlist.Count);
                destAP = APlist[index];
            }
            else
            {
                destAP = "UML";
            }
            
            //Generate speed info - Outbound start at 20 and increase to 200, Inbound start at random amount and  follow speed limit
            if (control == 'D')  
            {
                speed = 20;
                expectedSpeed = 200;
            }
            else
            {
                speed = random.Next(150, 350);
                if (speed > 250)
                {
                    expectedSpeed = 250;
                }
                else
                {
                    expectedSpeed = speed;
                }
            }
            //Generate altitude info - Outbound start at 0 and increase to 100,  Inbound start at random amount and follow ceiling        
            if (control == 'D') 
            {
                altitude = 0;
                expectedAltitude = 100;
            }
            else
            {
                altitude = random.Next(100, 200);
                expectedAltitude = 100;
            }
            //Set expected heading
            if (control == 'D')
            {
                if (index < 5)
                {
                    expectedHeading = 240;
                }
                else
                {
                    expectedHeading = 98;
                }
            }
            else
            {
                expectedHeading = heading;
            }

        }

        //Changes blank control indicators to A (only incomming aircraft have blank control indicators), shows border on selected aircraft, and displays directive input table
        public void Airplane_Click(object sender, EventArgs e)  
        {
            if (control == ' ')
            {
                control = 'A';
            }
            PictureBox pic = sender as PictureBox;
            foreach (Control item in f.Controls.OfType<TableLayoutPanel>())
            {
                if (item.Name == "tblPlaneInfo")
                {
                    f.Controls.Remove(item);
                    item.Dispose();
                }
                
            }
            foreach (PictureBox item in f.Controls.OfType <PictureBox>())
            {
                item.BorderStyle = BorderStyle.None;
            }
            Airplane.BorderStyle = BorderStyle.Fixed3D;
            tableMaker();
        }
        //Commits directives to expected heading, speed and altitude.  Also, closes input table and removes icon border.
        public void btnSendCmd_Click(object sender, EventArgs e, string newHeading, string newSpeed, string newAltitude)
        {
            foreach (Control item in f.Controls.OfType<TableLayoutPanel>())
            {
                if (item.Name == "tblPlaneInfo")
                {
                    f.Controls.Remove(item);
                    item.Dispose();
                }

            }
            Airplane.BorderStyle = BorderStyle.None;
            expectedHeading = Int32.Parse(newHeading);
            expectedSpeed = Int32.Parse(newSpeed);
            expectedAltitude = Int32.Parse(newAltitude);
        }
        //Performs rotation of airplane icon based on current heading angle
        private Bitmap rotateImage(Bitmap b, int angle)
        {
            int maxside = (int)(Math.Sqrt(b.Width * b.Width + b.Height * b.Height));
            Bitmap returnBitmap = new Bitmap(maxside, maxside);
            Graphics g = Graphics.FromImage(returnBitmap);

            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            g.DrawImage(b, new Point(0, 0));

            return returnBitmap;

        }
        //Scale movement in X and Y coordinates depending on heading angle
        private (double scaleX, double scaleY) vectorScale(int heading)
        {
            double scaleY = Math.Sin(heading * (Math.PI / 180));
            double scaleX = Math.Cos(heading * (Math.PI / 180));
            return (scaleX,scaleY);
        }

        public void tm_Tick(object sender, EventArgs e)
        {
            
            //Curtail speed changes to 20kts per update
            if (expectedSpeed == speed || Math.Abs(expectedSpeed - speed) < 20)
            {
                speed = expectedSpeed;
            }
            else if (expectedSpeed - speed < 0)
            {
                speed = speed + Math.Max(expectedSpeed - speed, -20);
            }
            else
            {
                speed = speed + Math.Min(expectedSpeed - speed, 20);
            }
            //Curtail altitude changes to 2000ft (20) per update
            if (expectedAltitude == altitude || Math.Abs(expectedAltitude - altitude) < 20)
            {
                altitude = expectedAltitude;
            }
            else if (expectedAltitude - altitude < 0)
            {
                altitude = altitude + Math.Max(expectedAltitude - altitude, -20);
            }
            else
            {
                altitude = altitude + Math.Min(expectedAltitude - altitude, 20);
            }
            //Curtail heading changes to 20 deg per update              FIXME: Need to account for turning right and left!  Maybe add a R or L radio button?
            double landingAngle = (Math.Atan2(Airplane.Location.Y - 285 + 10, Airplane.Location.X - 410 + 10) * (180 / Math.PI));
            if (heading == 220 && landingAngle > -58 && landingAngle < -50 && altitude <= 50 && (altitude >= 10 || landing == true))
            {
                double tx = 410 - Airplane.Location.X + 12;
                double ty = 285 - Airplane.Location.Y + 12;
                double l = Math.Sqrt(tx * tx + ty * ty);
                Airplane.Location = new Point(Airplane.Location.X + (int)(speed / 50 * (tx/l)), Airplane.Location.Y + (int)(speed / 50 * (ty/l)));  
                planeinfo.Location = new Point(planeinfo.Location.X + (int)(speed / 50 * (tx / l)), planeinfo.Location.Y + (int)(speed / 50 * (ty / l)));  //Use this to gauge speed difference - Uncomment when ready fro use
                landing = true;
            }
            else if (expectedHeading == heading || Math.Abs(expectedHeading - heading) < 20)
            {
                heading = expectedHeading;
            }
            else if (expectedHeading - heading < 0)
            {
                heading = heading + Math.Max(expectedHeading - heading, -20);
            }
            else
            {
                heading = heading + Math.Min(expectedHeading - heading, 20);
            }
            //Rotate image to match heading
            (double scaleX, double scaleY) = vectorScale(heading);
            Airplane.Image = rotateImage(bmp, heading);
            //Update plane and info location with new information
            Airplane.Location = new Point(Airplane.Location.X + (int)(speed/10 * scaleY), Airplane.Location.Y - (int)(speed/10 * scaleX));
            planeinfo.Location = new Point(planeinfo.Location.X + (int)(speed/10 * scaleY), planeinfo.Location.Y - (int)(speed/10 * scaleX));
            //Update plane info display with new info
            planeinfo.Text = String.Format("{0} {1} {2}\r\n{3} {4} {5}", Airplane.Tag, destAP, control, altitude, speed, heading);
            //When on approach incrementally decrease altitude and speed
            if (landing == true)
            {
                double distToRunway = (Math.Sqrt(Math.Pow(Airplane.Location.X - 410 + 10, 2) + Math.Pow(Airplane.Location.Y - 285 + 10, 2)));
                if (distToRunway < 15)
                {
                    f.Controls.Remove(Airplane);
                    f.Controls.Remove(planeinfo);
                    f.Controls.Remove(tblPlaneInfo);
                    
                }
                else if (distToRunway < 55)
                {
                    expectedAltitude = 5;
                    if (expectedSpeed > 125)
                    {
                        expectedSpeed = 125;
                    }
                    
                }
                else if (distToRunway < 100)
                {
                    expectedAltitude = 10;
                    if (expectedSpeed > 150)
                    {
                        expectedSpeed = 150;
                    }
                }
                else if (distToRunway < 150)
                {
                    if (expectedAltitude > 30)
                    {
                        expectedAltitude = 30;
                    }
                    if (expectedSpeed > 170)
                    {
                        expectedSpeed = 170;
                    }
                }
                else if (distToRunway < 200)
                {
                    if (expectedAltitude > 40)
                    {
                        expectedAltitude = 40;
                    }
                    if (expectedSpeed > 200)
                    {
                        expectedSpeed = 200;
                    }
                   
                }
            }  

        }

        public void tableMaker()
        {
            //f.InitializeComponent();
            
            TextBox txtIDTitle = new System.Windows.Forms.TextBox();
            TextBox txtID = new System.Windows.Forms.TextBox();
            TextBox txtSpdTitle = new System.Windows.Forms.TextBox();
            TextBox txtSpd = new System.Windows.Forms.TextBox();
            tblPlaneInfo = new System.Windows.Forms.TableLayoutPanel();
            TableLayoutPanel tblTurnDirection = new System.Windows.Forms.TableLayoutPanel();
            TextBox txtAlt = new System.Windows.Forms.TextBox();
            TextBox txtAltTitle = new System.Windows.Forms.TextBox();
            TextBox txtHead = new System.Windows.Forms.TextBox();
            TextBox txtHeadTitle = new System.Windows.Forms.TextBox();
            Button btnSendCmd = new System.Windows.Forms.Button();
            tblPlaneInfo.SuspendLayout();
            f.SuspendLayout();
            // 
            // txtIDTitle
            // 
            txtIDTitle.Location = new System.Drawing.Point(3, 3);
            txtIDTitle.Name = "txtIDTitle";
            txtIDTitle.ReadOnly = true;
            txtIDTitle.Size = new System.Drawing.Size(69, 26);
            txtIDTitle.TabIndex = 1;
            txtIDTitle.Text = "ID";
            // 
            // txtID
            // 
            txtID.Location = new System.Drawing.Point(78, 3);
            txtID.Name = "txtID";
            txtID.ReadOnly = true;
            txtID.Size = new System.Drawing.Size(132, 26);
            txtID.TabIndex = 2;
            txtID.Text = ID;
            // 
            // txtSpdTitle
            // 
            txtSpdTitle.Location = new System.Drawing.Point(3, 35);
            txtSpdTitle.Name = "txtSpdTitle";
            txtSpdTitle.ReadOnly = true;
            txtSpdTitle.Size = new System.Drawing.Size(69, 26);
            txtSpdTitle.TabIndex = 3;
            txtSpdTitle.Text = "Speed";
            // 
            // txtSpd
            // 
            txtSpd.Location = new System.Drawing.Point(78, 35);
            txtSpd.Name = "txtSpd";
            txtSpd.Size = new System.Drawing.Size(132, 26);
            txtSpd.TabIndex = 4;
            txtSpd.Text = speed.ToString();
            // 
            // tblPlaneInfo
            // 
            tblPlaneInfo.ColumnCount = 2;
            tblPlaneInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tblPlaneInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tblPlaneInfo.Controls.Add(btnSendCmd, 1, 5);
            tblPlaneInfo.Controls.Add(tblTurnDirection, 1, 4); //FIXME: Add layoutTable with two radio buttons for CW and CCW turns.  Also, add "Turn Direction" title to first column!
            tblPlaneInfo.Controls.Add(txtHead, 1, 3);
            tblPlaneInfo.Controls.Add(txtAltTitle, 0, 2);
            tblPlaneInfo.Controls.Add(txtAlt, 1, 2);
            tblPlaneInfo.Controls.Add(txtSpd, 1, 1);
            tblPlaneInfo.Controls.Add(txtSpdTitle, 0, 1);
            tblPlaneInfo.Controls.Add(txtID, 1, 0);
            tblPlaneInfo.Controls.Add(txtIDTitle, 0, 0);
            tblPlaneInfo.Controls.Add(txtHeadTitle, 0, 3);
            tblPlaneInfo.Name = "tblPlaneInfo";
            tblPlaneInfo.RowCount = 6;
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tblPlaneInfo.Size = new System.Drawing.Size(217, 141);
            tblPlaneInfo.Location = new System.Drawing.Point(0, f.ClientSize.Height - tblPlaneInfo.Bottom);
            tblPlaneInfo.TabIndex = 0;
            //
            //tblTurnDirection
            //
            /********FINISH THIS***********/
            //FIXME: Add CW radio button
            //FIXME: Add CCW radio button
            //FIXME: Add Turn Direction title
            // 
            // txtAlt
            // 
            txtAlt.Location = new System.Drawing.Point(78, 67);
            txtAlt.Name = "txtAlt";
            txtAlt.Size = new System.Drawing.Size(132, 26);
            txtAlt.TabIndex = 5;
            txtAlt.Text = altitude.ToString();
            // 
            // txtAltTitle
            // 
            txtAltTitle.Location = new System.Drawing.Point(3, 67);
            txtAltTitle.Name = "txtAltTitle";
            txtAltTitle.ReadOnly = true;
            txtAltTitle.Size = new System.Drawing.Size(69, 26);
            txtAltTitle.TabIndex = 6;
            txtAltTitle.Text = "Altitude";
            // 
            // txtHead
            // 
            txtHead.Location = new System.Drawing.Point(78, 99);
            txtHead.Name = "txtHead";
            txtHead.Size = new System.Drawing.Size(132, 26);
            txtHead.TabIndex = 7;
            txtHead.Text = heading.ToString();
            // 
            // txtHeadTitle
            // 
            txtHeadTitle.Location = new System.Drawing.Point(3, 99);
            txtHeadTitle.Name = "txtHeadTitle";
            txtHeadTitle.ReadOnly = true;
            txtHeadTitle.Size = new System.Drawing.Size(69, 26);
            txtHeadTitle.TabIndex = 8;
            txtHeadTitle.Text = "Heading";
            // 
            // btnSendCmd
            // 
            btnSendCmd.Location = new System.Drawing.Point(78, 131);
            btnSendCmd.Name = "btnSendCmd";
            btnSendCmd.Size = new System.Drawing.Size(132, 26);
            btnSendCmd.TabIndex = 1;
            btnSendCmd.Text = "Send Command";
            btnSendCmd.UseVisualStyleBackColor = true;
            btnSendCmd.Click += new EventHandler((sender, e) => btnSendCmd_Click(sender, e, txtHead.Text, txtSpd.Text, txtAlt.Text ));
            //
            // Add table control
            //
            f.Controls.Add(tblPlaneInfo);
            tblPlaneInfo.ResumeLayout(false);
            tblPlaneInfo.PerformLayout();
            f.ResumeLayout(false);
            f.PerformLayout();
        }
    }

    
    }
