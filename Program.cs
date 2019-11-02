/********************************************************************************************
* Copyright (c) Global Engineering Management by Invensys (refer to readme.txt for details)
* 
* Purpose           :  TIMIT annotation maker.
* 
* Rev  Date         By      Purpose
* ---- ------------ ------------------------------------------------------------------
* 01   2019-11-02   AG     the initial version. Including Transient/non transient version
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIMITAnnotationMaker
{
    class Program
    {
        enum AnnotationStamp
        {
            sil ,
            Tr,
            NTr
        }
        static void Main(string[] args)
        {
            string rootFolder = @"C:\Research\TIMIT\TIMIT";
            string fileExtension = "*.phn";
            double dlta = 10.0; // (+/-) Perecentage deviation off the marker line
            string[] targetAnnotations = { "sil", "Tr", "NTr" };
            //Assuming first and last annotations in each file is a sil
            string outputFolder = @"C:\Research\Output";
            //1- Get the list of files included into the modifcations
            string[] annotationFiles = System.IO.Directory.GetFiles(rootFolder, fileExtension, System.IO.SearchOption.AllDirectories);
            Console.WriteLine("Start...");
            int n = 1;
            foreach(string file in annotationFiles)
            {
                Console.Write("["+ n++ +" out of "+annotationFiles.Length+"]"+ file + " is being processed...");
                string[] annotations = System.IO.File.ReadAllLines(file);
                string[] modifiedAnnotations = ModifyAnnotations( annotations,dlta);
                string targetFile = GetTargetFileName(file, outputFolder,rootFolder,out string folder,out string targetWAVFileName);
                if(!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
                System.IO.File.WriteAllLines(targetFile, modifiedAnnotations);
                string wavFile = file.Substring(0,file.LastIndexOf('.')) + ".WAV";
                if(!System.IO.File.Exists(targetWAVFileName) && System.IO.File.Exists(wavFile)) System.IO.File.Copy(wavFile, targetWAVFileName);
                Console.WriteLine("...>End processing " + file);

            }

            
        }

        private static string GetTargetFileName(string file, string outputFolder, string rootFolder,out string folder, out string targetWAVFileName)
        {
            string baseFileName = System.IO.Path.GetFileNameWithoutExtension(file);
            folder = outputFolder+ System.IO.Path.GetDirectoryName(file).Substring(rootFolder.Length)+@"\";
            targetWAVFileName = folder  + baseFileName + ".WAV";
            string targetFileName = folder  + System.IO.Path.GetFileName(file);
            return targetFileName;
            

        }
       

        private static string[] ModifyAnnotations( string[] annotations, double dlta)
        {
            List<string> res = new List<string>();
            string modifiedAnnotation = string.Empty;
            string insertedAnnotation = string.Empty;
            int n, m, L;
            modifiedAnnotation = TrimRight(annotations[0], dlta,out int sampleIndex);
            modifiedAnnotation = StampAnnotation(modifiedAnnotation, AnnotationStamp.sil);
            res.Add(modifiedAnnotation);
            for (int i=1;i< annotations.Length-1;i++)
            {
                n = sampleIndex;
                modifiedAnnotation = TrimLeft(annotations[i], dlta, out sampleIndex);
                m = sampleIndex;
                modifiedAnnotation = TrimRight(modifiedAnnotation, dlta, out sampleIndex);
                L = sampleIndex;
                insertedAnnotation = CreateAnnotation(n, m, AnnotationStamp.Tr);
                modifiedAnnotation = StampAnnotation(modifiedAnnotation, AnnotationStamp.NTr);
                res.Add(insertedAnnotation);
                res.Add(modifiedAnnotation);
                sampleIndex = L;
            }
            n = sampleIndex;
            modifiedAnnotation = TrimLeft(annotations[annotations.Length - 1], dlta, out  sampleIndex);
            m = sampleIndex;
            insertedAnnotation = CreateAnnotation(n, m, AnnotationStamp.Tr);
            modifiedAnnotation = StampAnnotation(modifiedAnnotation, AnnotationStamp.sil);
            res.Add(insertedAnnotation);
            res.Add(modifiedAnnotation);

            return res.ToArray();
        }

        private static string CreateAnnotation(int n, int m, AnnotationStamp stamp)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", n, m, stamp.ToString());
            return sb.ToString();
        }

        private static string TrimLeft(string annotaion, double dlta, out int sampleIndex)
        {
            string[] seperators = { " ", "\t" };
            string[] parts = annotaion.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            int left = int.Parse(parts[0]);
            int right = int.Parse(parts[1]);
            int samples = Math.DivRem((right - left) * (int)(dlta + 0.5), 100, out var q);
            left += samples;
            sampleIndex = left;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", left, right, parts[2]);
            return sb.ToString();
        }

        private static string TrimRight(string annotaion, double dlta, out int sampleIndex)
        {


            string[] seperators = { " ", "\t" };
            string[] parts = annotaion.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            int left = int.Parse(parts[0]);
            int right = int.Parse(parts[1]);
            int samples = Math.DivRem((right-left) * (int)(dlta + 0.5), 100, out var q);
            right -=  samples;
            sampleIndex = right;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", left, right, parts[2]);
            return sb.ToString();
        }

        private static string StampAnnotation(string annotaion, AnnotationStamp stamp)
        {
            string[] seperators = { " ", "\t" };
            string[] parts = annotaion.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t{1}\t{2}", parts[0], parts[1], stamp.ToString());
            return sb.ToString();

        }
    }
}
