using Android.App;
using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Java.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XamarinARCore.Rendering
{
	/// <summary>
	/// Shader helper functions.
	/// </summary>
	public class ShaderUtil
	{
		/// <summary>
		/// Metodo que irá carregar o Shader
		/// </summary>
		/// <param name="context">Contexto</param>
		/// <param name="type">Tipo do shader que será criado</param>
		/// <param name="filename">O nome do asset que virá a ser um shader</param>
		/// <param name="defineValuesMap">Define os valores que serão adicionados no topo do shader source code.</param>
		/// <returns></returns>
		public static int loadGLShader(string tag, Context context, int type, string filename, Dictionary<string, int> defineValuesMap)
		{
			//Load shader source code.
			string code = readShaderFileFromAssets(context, filename);

			// Prepend any #define values specified during this run.
			string defines = "";

			foreach (var item in defineValuesMap)
			{
				defines += "#defines " + item.Key + " " + item.Value + "\n";
			}

			code = defines + code;

			// Compiles shader code.
			int shader = GLES20.GlCreateShader(type);
			GLES20.GlShaderSource(shader, code);
			GLES20.GlCompileShader(shader);

			// Get the compilation status.
			int[] compileStatus = new int[1];
			GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compileStatus, 0);

			// If the compilation failed, delete the shader.
			if (compileStatus[0] == 0)
			{
				Android.Util.Log.Error(tag, "Error compiling shader: " + GLES20.GlGetShaderInfoLog(shader));
				GLES20.GlDeleteShader(shader);
				shader = 0;
			}

			if (shader == 0)
			{
				throw new RuntimeException("Error creating shader.");
			}

			return shader;
		}

		/** Overload of loadGLShader that assumes no additional #define values to add. */
		public static int loadGLShader(string tag, Context context, int type, string filename)
		{
			try
			{
				Dictionary<string, int> emptyDefineValuesMap = new Dictionary<string, int>();
				return loadGLShader(tag, context, type, filename, emptyDefineValuesMap);
			}
			catch (System.Exception e)
			{
				throw;
			}
		}

		/**
		* Checks if we've had an error inside of OpenGL ES, and if so what that error is.
		*
		* @param label Label to report in case of error.
		* @throws RuntimeException If an OpenGL error is detected.
		**/
		public static void checkGLError(string tag, string label)
		{
			int lastError = GLES20.GlNoError;
			// Drain the queue of all errors.
			int error;
			while ((error = GLES20.GlGetError()) != GLES20.GlNoError)
			{
				Android.Util.Log.Error(tag, label + ": glError " + error);
				lastError = error;
			}
			if (lastError != GLES20.GlNoError)
			{
				throw new RuntimeException(label + ": glError " + lastError);
			}
		}

		/**
        * Converts a raw shader file into a string.
        *
        * @param filename The filename of the shader file about to be turned into a shader.
        * @return The context of the text file, or null in case of error.
        */
		private static string readShaderFileFromAssets(Context context, string filename)
		{
			using (Stream inputStream = context.Assets.Open(filename))
			{
				BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream));

				Java.Lang.StringBuilder sb = new Java.Lang.StringBuilder();
				string line;

				int cont = 0;

				while ((line = reader.ReadLine()) != null)
				{
					string[] tokens = line.Split(" ");

					if (tokens[0].Equals("#include"))
					{
						string includeFilename = tokens[1];
						includeFilename = includeFilename.Replace("\"", "");

						if (includeFilename.Equals(filename))
						{
							throw new System.IO.IOException("Do not include the calling file.");
						}

						sb.Append(readShaderFileFromAssets(context, includeFilename));
					}
					else
					{
						sb.Append(line).Append("\n");
					}

					cont++;
					System.Console.WriteLine("Contador: "+cont);

				}

				return sb.ToString();
			}
		}

		private ShaderUtil() { }

	}
}